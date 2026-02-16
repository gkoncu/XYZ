using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession
{
    public class UpdateClassSessionCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Update;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int SessionId { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? CoachNote { get; set; }

        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    }
}
