using MediatR;
using System;

namespace XYZ.Application.Features.Admins.Commands.CreateAdmin
{
    public sealed class CreateAdminCommand : IRequest<int>
    {
        public string UserId { get; set; } = string.Empty;

        public int? TenantId { get; set; }

        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        public string? IdentityNumber { get; set; }

        public bool CanManageUsers { get; set; } = true;
        public bool CanManageFinance { get; set; } = true;
        public bool CanManageSettings { get; set; } = true;
    }
}
