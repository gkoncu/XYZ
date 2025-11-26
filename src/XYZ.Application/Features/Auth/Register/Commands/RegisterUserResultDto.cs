namespace XYZ.Application.Features.Auth.Register.Commands
{
    public sealed class RegisterUserResultDto
    {
        public string UserId { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public int TenantId { get; init; }
        public string TempPassword { get; init; } = string.Empty;
    }
}
