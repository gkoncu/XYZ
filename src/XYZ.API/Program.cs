using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text;
using XYZ.API.Dev;
using XYZ.API.HostedServices;
using XYZ.API.Services.Auth;
using XYZ.API.Services.Email;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Application.Data;
using XYZ.Application.Features.Auth.Options;
using XYZ.Application.Features.Email.Options;
using XYZ.Application.Services;
using XYZ.Application.Services.Auth;
using XYZ.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- DbContext & Identity ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        // TODO : Lockout/Confirm
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- Options (Jwt) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// --- Options (Email) ---
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

// --- Email sender ---
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// --- Authentication: JWT Bearer ---
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"]!;
var audience = jwtSection["Audience"]!;
var signingKey = jwtSection["SigningKey"]!;

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = true;
        o.SaveToken = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// --- MediatR & FluentValidation (Application assembly scan) ---
var appAssembly = typeof(XYZ.Application.Features.Auth.Login.Commands.LoginCommand).Assembly;

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(appAssembly));

ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr-TR");
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(appAssembly);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);


// --- IHttpContextAccessor + CurrentUserService ---
builder.Services.AddHttpContextAccessor();

// --- Application Services (Auth) ---
builder.Services.AddScoped<IDataScopeService, DataScopeService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserLookup, UserLookupService>();
builder.Services.AddScoped<IPasswordSignIn, PasswordSignInService>();
builder.Services.AddScoped<IJwtFactory, JwtFactory>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<IRoleAssignmentService, RoleAssignmentService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPasswordSetupLinkBuilder, PasswordSetupLinkBuilder>();

// Payments: scheduled overdue marker
builder.Services.AddScoped<IOverduePaymentService, OverduePaymentService>();
builder.Services.AddHostedService<OverduePaymentsHostedService>();

builder.Services.AddControllers();

// --- Swagger Dev---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "XYZ API", Version = "v1" });

    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });

    c.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT Bearer token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.CustomSchemaIds(type => type.FullName);
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await DevIdentitySeeder.RunAsync(app.Services);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
