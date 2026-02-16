using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachDocumentStatus
{
    public class GetCoachDocumentStatusQuery : IRequest<UserDocumentStatusDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int CoachId { get; set; }
    }
}
