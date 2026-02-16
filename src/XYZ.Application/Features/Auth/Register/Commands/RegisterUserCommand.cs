using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Auth.Register.Commands
{
    public sealed class RegisterUserCommand : IRequest<RegisterUserResultDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Users.Create;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public int TenantId { get; set; }
    }
}
