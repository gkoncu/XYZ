using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachesDocumentStatus
{
    public class GetCoachesDocumentStatusQuery : IRequest<IList<CoachDocumentStatusListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Read;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public bool OnlyIncomplete { get; set; }
        public string? SearchTerm { get; set; }
        public int Take { get; set; } = 200;
    }
}
