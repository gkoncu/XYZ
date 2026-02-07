using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using XYZ.Web.Infrastructure;
using XYZ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TenantThemeFilter>();

builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<TenantThemeFilter>();
});

builder.Services
    .AddHttpClient<IApiClient, ApiClient>(client =>
    {
        var baseUrl = builder.Configuration["Api:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Api:BaseUrl is not configured in appsettings.json");
        }

        client.BaseAddress = new Uri(baseUrl);
    })
    .AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiNoAuth", client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Api:BaseUrl is not configured in appsettings.json");
    }

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

app.UseExceptionHandler("/Home/Error");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
