using System;
using XYZ.Web.Infrastructure;
using XYZ.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

// TenantThemeFilter
builder.Services.AddScoped<TenantThemeFilter>();

builder.Services.AddControllersWithViews(options =>
{
    // Tüm MVC action'larýnda theme otomatik yüklensin
    options.Filters.AddService<TenantThemeFilter>();
});

// Typed HttpClient -> ApiClient
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("Api:BaseUrl is not configured in appsettings.json");
    }

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=AdminDashboard}/{action=Index}/{id?}");

app.Run();
