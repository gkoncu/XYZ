using MediatR;
using System;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Users.Update;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int AdminId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        public string? IdentityNumber { get; set; }

        public bool CanManageUsers { get; set; }
        public bool CanManageFinance { get; set; }
        public bool CanManageSettings { get; set; }
    }
}
