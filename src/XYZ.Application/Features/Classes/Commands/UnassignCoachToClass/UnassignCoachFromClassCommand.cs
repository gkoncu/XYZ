using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.UnassignCoachToClass
{
    public class UnassignCoachFromClassCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.AssignCoach;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int CoachId { get; set; }
        public int? ClassId { get; set; }
    }
}
