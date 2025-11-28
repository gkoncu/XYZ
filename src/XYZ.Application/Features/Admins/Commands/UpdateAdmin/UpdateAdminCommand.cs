using MediatR;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommand : IRequest<int>
    {
        public int AdminId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;

        public bool CanManageUsers { get; set; }
        public bool CanManageFinance { get; set; }
        public bool CanManageSettings { get; set; }
    }
}
