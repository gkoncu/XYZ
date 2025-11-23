using System.Net.Http.Headers;

namespace XYZ.Web.Infrastructure
{
    public class ApiAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAuthorizationMessageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var token = user.FindFirst("access_token")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
