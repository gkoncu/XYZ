using Microsoft.AspNetCore.Http;

namespace XYZ.API.Services.Auth;

public static class PasswordSetupHeaders
{
    public static void Write(HttpResponse response, string userId, string token, string? setupUrl)
    {
        response.Headers["X-Password-UserId"] = userId;
        response.Headers["X-Password-Token"] = token;

        if (!string.IsNullOrWhiteSpace(setupUrl))
            response.Headers["X-Password-Setup-Url"] = setupUrl;
    }
}
