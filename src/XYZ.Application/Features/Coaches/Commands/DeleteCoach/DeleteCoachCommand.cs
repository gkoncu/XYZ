using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach
{
    public sealed class DeleteCoachCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Coaches.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int CoachId { get; set; }
    }
}
