using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Infrastructure
{
    public class TenantThemeFilter : IAsyncActionFilter
    {
        private const string CookieName = "TenantTheme";
        private const string HttpContextItemKey = "TenantTheme";

        private readonly IApiClient _apiClient;

        public TenantThemeFilter(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            TenantThemeViewModel? theme = null;

            if (http.Request.Cookies.TryGetValue(CookieName, out var cookieValue)
                && !string.IsNullOrWhiteSpace(cookieValue))
            {
                try
                {
                    theme = JsonSerializer.Deserialize<TenantThemeViewModel>(cookieValue);
                }
                catch
                {
                }
            }

            if (theme == null)
            {
                try
                {
                    theme = await _apiClient.GetCurrentTenantThemeAsync();
                }
                catch
                {
                    theme = new TenantThemeViewModel();
                }

                var json = JsonSerializer.Serialize(theme);

                http.Response.Cookies.Append(
                    CookieName,
                    json,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        IsEssential = true,
                        Expires = DateTimeOffset.UtcNow.AddHours(8)
                    });
            }

            http.Items[HttpContextItemKey] = theme;

            if (context.Controller is Controller controller && theme != null)
            {
                controller.ViewData["PrimaryColor"] = theme.PrimaryColor;
                controller.ViewData["SecondaryColor"] = theme.SecondaryColor;
                controller.ViewData["TenantName"] = theme.Name;
                controller.ViewData["LogoUrl"] = theme.LogoUrl;
            }

            await next();
        }

        public static TenantThemeViewModel? GetThemeFromHttpContext(HttpContext httpContext)
        {
            return httpContext.Items.TryGetValue(HttpContextItemKey, out var value)
                ? value as TenantThemeViewModel
                : null;
        }
    }
}
