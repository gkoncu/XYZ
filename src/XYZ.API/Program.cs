using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using XYZ.API.Dev;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Application.Data;
using XYZ.Application.Features.Auth.Options;
using XYZ.Application.Services.Auth;
using XYZ.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- DbContext & Identity ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        // TODO : Lockout/Confirm
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- Options (Jwt) ---
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(appAssembly);

// --- IHttpContextAccessor + CurrentUserService ---
builder.Services.AddHttpContextAccessor();
// builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- Application Services (Auth) ---
// Interface -> Implementation Matchup
builder.Services.AddScoped<IUserLookup, UserLookupService>();
builder.Services.AddScoped<IPasswordSignIn, PasswordSignInService>();
builder.Services.AddScoped<IJwtFactory, JwtFactory>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

builder.Services.AddControllers();

// --- Swagger Dev---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "XYZ API", Version = "v1" });
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