using MediatR;

namespace XYZ.Application.Features.Auth.Register.Commands
{
    public sealed class RegisterUserCommand : IRequest<RegisterUserResultDto>
    {
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public int TenantId { get; set; }
    }
}
