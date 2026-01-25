using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using XYZ.Web.Models.Theming;
using XYZ.Web.Services;

namespace XYZ.Web.Infrastructure
{
    public sealed class TenantThemeFilter : IAsyncActionFilter
    {
        private const string ThemeCookieName = "TenantTheme";

        private readonly IApiClient _api;

        public TenantThemeFilter(IApiClient api)
        {
            _api = api;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is not Controller controller)
            {
                await next();
                return;
            }

            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                ApplyTheme(controller, primary: null, secondary: null, logoUrl: null);
                await next();
                return;
            }

            var tenantId = GetTenantId(user);

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                ApplyTheme(controller, primary: null, secondary: null, logoUrl: null);
                await next();
                return;
            }

            if (context.HttpContext.Request.Cookies.TryGetValue(ThemeCookieName, out var cookieValue)
                && TryParseCookie(cookieValue, out var cached)
                && string.Equals(cached.TenantId, tenantId, StringComparison.OrdinalIgnoreCase)
                && IsValidColor(cached.PrimaryColor)
                && IsValidColor(cached.SecondaryColor))
            {
                ApplyTheme(controller, cached.PrimaryColor, cached.SecondaryColor, cached.LogoUrl);
                await next();
                return;
            }

            if (context.HttpContext.Request.Cookies.ContainsKey(ThemeCookieName))
            {
                context.HttpContext.Response.Cookies.Delete(ThemeCookieName);
            }

            var theme = await _api.GetCurrentTenantThemeRawAsync(context.HttpContext.RequestAborted);

            if (theme is null || !IsValidColor(theme.PrimaryColor) || !IsValidColor(theme.SecondaryColor))
            {
                ApplyTheme(controller, primary: null, secondary: null, logoUrl: null);
                await next();
                return;
            }

            ApplyTheme(controller, theme.PrimaryColor, theme.SecondaryColor, theme.LogoUrl);

            var payload = new ThemeCookiePayload
            {
                TenantId = tenantId,
                PrimaryColor = theme.PrimaryColor,
                SecondaryColor = theme.SecondaryColor,
                LogoUrl = theme.LogoUrl
            };

            WriteCookie(context.HttpContext.Response, payload);

            await next();
        }

        private static void ApplyTheme(Controller controller, string? primary, string? secondary, string? logoUrl)
        {
            controller.ViewData["PrimaryColor"] = primary;
            controller.ViewData["SecondaryColor"] = secondary;
            controller.ViewData["LogoUrl"] = logoUrl;
        }

        private static string? GetTenantId(ClaimsPrincipal user)
        {
            return user.FindFirstValue("tenant_id");
        }

        private static void WriteCookie(HttpResponse response, ThemeCookiePayload payload)
        {
            var json = JsonSerializer.Serialize(payload);

            response.Cookies.Append(
                ThemeCookieName,
                json,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });
        }

        private static bool TryParseCookie(string cookieValue, out ThemeCookiePayload payload)
        {
            payload = new ThemeCookiePayload();

            try
            {
                var parsed = JsonSerializer.Deserialize<ThemeCookiePayload>(cookieValue);
                if (parsed is null) return false;

                payload = parsed;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidColor(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.Length != 7 || value[0] != '#') return false;

            for (int i = 1; i < 7; i++)
            {
                var c = value[i];
                var isHex = (c >= '0' && c <= '9')
                            || (c >= 'a' && c <= 'f')
                            || (c >= 'A' && c <= 'F');
                if (!isHex) return false;
            }

            return true;
        }

        private sealed class ThemeCookiePayload
        {
            public string TenantId { get; set; } = "";
            public string PrimaryColor { get; set; } = "";
            public string SecondaryColor { get; set; } = "";
            public string? LogoUrl { get; set; }
        }

        public static TenantThemeViewModel? GetThemeFromHttpContext(HttpContext httpContext)
        {
            if (httpContext == null) return null;

            var currentTenantId =
                httpContext.User.FindFirst("TenantId")?.Value
                ?? httpContext.User.FindFirst("tenantId")?.Value
                ?? httpContext.User.FindFirst("tenant_id")?.Value;

            if (string.IsNullOrWhiteSpace(currentTenantId))
                return null;

            if (!httpContext.Request.Cookies.TryGetValue("TenantTheme", out var cookieValue))
                return null;

            if (!TryParseCookie(cookieValue, out var cached))
                return null;

            if (!string.Equals(cached.TenantId, currentTenantId, StringComparison.OrdinalIgnoreCase))
                return null;

            if (!IsValidColor(cached.PrimaryColor) || !IsValidColor(cached.SecondaryColor))
                return null;

            return new TenantThemeViewModel
            {
                Name = "",
                PrimaryColor = cached.PrimaryColor,
                SecondaryColor = cached.SecondaryColor,
                LogoUrl = cached.LogoUrl
            };
        }

    }
}
