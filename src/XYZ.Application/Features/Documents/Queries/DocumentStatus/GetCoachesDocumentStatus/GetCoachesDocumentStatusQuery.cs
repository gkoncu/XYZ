using MediatR;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachesDocumentStatus
{
    public class GetCoachesDocumentStatusQuery : IRequest<IList<CoachDocumentStatusListItemDto>>
    {
        public bool OnlyIncomplete { get; set; }
        public string? SearchTerm { get; set; }
        public int Take { get; set; } = 200;
    }
}
