using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentsDocumentStatus
{
    public class GetStudentsDocumentStatusQuery : IRequest<IList<StudentDocumentStatusListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Read;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public bool OnlyIncomplete { get; set; }
        public string? SearchTerm { get; set; }
        public int Take { get; set; } = 200;
    }
}
