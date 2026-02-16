using MediatR;
using System;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.UpdateCoach
{
    public class UpdateCoachCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Coaches.Update;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int CoachId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; } = "PreferNotToSay";

        public string BloodType { get; set; } = "Unknown";

        public string? IdentityNumber { get; set; }

        public string? LicenseNumber { get; set; }

        public int BranchId { get; set; }
    }
}
