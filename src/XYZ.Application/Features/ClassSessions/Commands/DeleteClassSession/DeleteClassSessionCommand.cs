using MediatR;

namespace XYZ.Application.Features.ClassSessions.Commands.DeleteClassSession
{
    public class DeleteClassSessionCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
