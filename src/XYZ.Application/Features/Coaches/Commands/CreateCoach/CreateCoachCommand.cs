using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public sealed class CreateCoachCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Coaches.Create;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public string UserId { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public string? IdentityNumber { get; set; }

        public string? LicenseNumber { get; set; }
    }
}
