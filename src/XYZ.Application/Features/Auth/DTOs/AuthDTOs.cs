using System.ComponentModel.DataAnnotations;

namespace XYZ.Application.Features.Auth.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Email adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Þifre zorunludur")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class LoginResponse
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed record LoginResultDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc,

    string UserId,
    string Email,
    string FullName,
    string[] Roles,
    string? TenantId
);
