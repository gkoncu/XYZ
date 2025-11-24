using MediatR;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById
{
    public class GetClassSessionByIdQuery : IRequest<ClassSessionDetailDto>
    {
        public int Id { get; set; }
    }
}
