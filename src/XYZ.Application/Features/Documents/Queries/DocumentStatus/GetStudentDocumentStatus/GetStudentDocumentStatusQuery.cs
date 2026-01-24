using MediatR;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentDocumentStatus
{
    public class GetStudentDocumentStatusQuery : IRequest<UserDocumentStatusDto>
    {
        public int StudentId { get; set; }
    }
}
