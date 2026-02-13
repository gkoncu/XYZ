using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace XYZ.API.Middleware
{
    /// <summary>
    /// Tenant-scoped endpoint'lerde (çoğu API çağrısı) authenticated user için TenantId claim zorunlu.
    /// Exempt: /api/auth/*, /swagger*, host-level /api/tenants (ama /api/tenants/current-theme hariç).
    /// </summary>
    public sealed class TenantContextRequiredMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantContextRequiredMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (IsExemptPath(path))
            {
                await _next(context);
                return;
            }

            var tenantClaim = context.User.FindFirstValue("TenantId");
            if (!int.TryParse(tenantClaim, out var tenantId) || tenantId <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant context required.",
                    detail = "Bu endpoint için TenantId zorunludur. Lütfen tenant seçip tekrar deneyin."
                });
                return;
            }

            await _next(context);
        }

        private static bool IsExemptPath(string path)
        {
            if (path.StartsWith("/swagger"))
                return true;

            if (path.StartsWith("/api/auth"))
                return true;

            if (path.StartsWith("/api/tenants") && !path.StartsWith("/api/tenants/current-theme"))
                return true;

            return false;
        }
    }
}
