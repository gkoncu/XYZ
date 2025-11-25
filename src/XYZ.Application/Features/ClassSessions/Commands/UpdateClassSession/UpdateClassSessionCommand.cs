using MediatR;
using System;

namespace XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession
{
    public class UpdateClassSessionCommand : IRequest<int>
    {
        public int SessionId { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Location { get; set; }

        public string? CoachNote { get; set; }
    }
}
