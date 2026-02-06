using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using XYZ.Application.Features.Auth.DTOs;

namespace XYZ.Web.Infrastructure
{
    /// <summary>
    /// Web -> API çağrılarında Authorization header'ını otomatik ekler.
    /// 401 dönerse refresh token cookie ile /auth/refresh çağırır, access_token claim'ini yeniler ve isteği 1 kez tekrar dener.
    /// </summary>
    public sealed class ApiAuthorizationMessageHandler : DelegatingHandler
    {
        private const string RefreshTokenCookieName = "xyz_rt";
        private static readonly HttpRequestOptionsKey<bool> RetriedKey = new("xyz_auth_retried");

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiAuthorizationMessageHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            var accessToken = user?.FindFirst("access_token")?.Value;
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            if (request.Options.TryGetValue(RetriedKey, out var alreadyRetried) && alreadyRetried)
            {
                return response;
            }

            if (httpContext is null || user?.Identity?.IsAuthenticated != true)
            {
                return response;
            }

            if (!httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken)
                || string.IsNullOrWhiteSpace(refreshToken))
            {
                return response;
            }

            var refreshed = await TryRefreshAsync(httpContext, refreshToken, cancellationToken);
            if (refreshed is null)
            {
                return response;
            }

            response.Dispose();

            var retryRequest = await CloneHttpRequestMessageAsync(request, cancellationToken);
            retryRequest.Options.Set(RetriedKey, true);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);

            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private async Task<LoginResultDto?> TryRefreshAsync(HttpContext httpContext, string refreshToken, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient("ApiNoAuth");

            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsJsonAsync("auth/refresh", new { RefreshToken = refreshToken }, ct);
            }
            catch
            {
                return null;
            }

            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await resp.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: ct);
            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                return null;
            }

            httpContext.Response.Cookies.Append(RefreshTokenCookieName, result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            var auth = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!auth.Succeeded || auth.Principal is null)
            {
                return result;
            }

            var principal = auth.Principal;
            var identity = principal.Identities.FirstOrDefault(i => i.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
                           ?? (principal.Identity as ClaimsIdentity);

            if (identity is null)
            {
                return result;
            }

            ReplaceClaim(identity, "access_token", result.AccessToken);
            if (!string.IsNullOrWhiteSpace(result.TenantId))
            {
                ReplaceClaim(identity, "tenant_id", result.TenantId);
            }

            var props = auth.Properties ?? new AuthenticationProperties();
            props.ExpiresUtc = result.ExpiresAtUtc;

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props);

            return result;
        }

        private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string newValue)
        {
            var existing = identity.FindFirst(claimType);
            if (existing is not null)
            {
                identity.RemoveClaim(existing);
            }
            identity.AddClaim(new Claim(claimType, newValue));
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                await using var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms, ct);
                ms.Position = 0;

                var contentBytes = ms.ToArray();
                var newContent = new ByteArrayContent(contentBytes);

                foreach (var header in request.Content.Headers)
                {
                    newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                clone.Content = newContent;
            }

            foreach (var opt in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(opt.Key), opt.Value);
            }

            return clone;
        }
    }
}
