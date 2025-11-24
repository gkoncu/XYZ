using MediatR;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.ChangeSessionStatus
{
    public class ChangeSessionStatusCommand : IRequest<int>
    {
        public int SessionId { get; set; }
        public SessionStatus Status { get; set; }
    }
}
