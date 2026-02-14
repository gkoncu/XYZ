using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.ChangeSessionStatus
{
    public class ChangeSessionStatusCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.ChangeStatus;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int SessionId { get; set; }
        public SessionStatus Status { get; set; }
    }
}
