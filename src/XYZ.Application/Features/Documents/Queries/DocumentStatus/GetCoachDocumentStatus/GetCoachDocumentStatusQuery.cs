using MediatR;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachDocumentStatus
{
    public class GetCoachDocumentStatusQuery : IRequest<UserDocumentStatusDto>
    {
        public int CoachId { get; set; }
    }
}
