using System;
using Microsoft.Extensions.Configuration;
using XYZ.Application.Common.Interfaces;

namespace XYZ.API.Services.Auth;

public sealed class PasswordSetupLinkBuilder(IConfiguration config) : IPasswordSetupLinkBuilder
{
    public string? Build(string userId, string token)
    {
        var webBaseUrl = config["Web:BaseUrl"];
        if (string.IsNullOrWhiteSpace(webBaseUrl))
            return null;

        return $"{webBaseUrl.TrimEnd('/')}/Account/SetPassword?uid={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
    }
}
