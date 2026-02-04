using Microsoft.AspNetCore.Mvc.Filters;
using XYZ.Web.Services;

namespace XYZ.Web.Infrastructure.Filters;

public sealed class EnsureProfileCookieFilter(IApiClient apiClient, IConfiguration config) : IAsyncActionFilter
{
    private readonly IApiClient _api = apiClient;
    private readonly IConfiguration _config = config;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        if (!(http.User?.Identity?.IsAuthenticated ?? false))
        {
            await next();
            return;
        }

        var existing = http.Request.Cookies["xyz_pp_url"];
        if (!string.IsNullOrWhiteSpace(existing))
        {
            await next();
            return;
        }

        try
        {
            var dto = await _api.GetMyProfileAsync(http.RequestAborted);
            if (dto is null || string.IsNullOrWhiteSpace(dto.ProfilePictureUrl))
            {
                await next();
                return;
            }

            var normalized = NormalizeAssetUrl(dto.ProfilePictureUrl);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                await next();
                return;
            }

            http.Response.Cookies.Append(
                "xyz_pp_url",
                normalized,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true
                });
        }
        catch
        {
        }

        await next();

        string? NormalizeAssetUrl(string raw)
        {
            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return raw;

            var baseUrl = _config["Api:BaseUrl"] ?? string.Empty;
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiUri))
                return raw;

            var origin = apiUri.GetLeftPart(UriPartial.Authority);

            if (!raw.StartsWith("/"))
                raw = "/" + raw;

            return $"{origin}{raw}?v={DateTime.UtcNow.Ticks}";
        }
    }
}
