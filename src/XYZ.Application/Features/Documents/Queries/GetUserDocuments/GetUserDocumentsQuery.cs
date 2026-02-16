using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetUserDocuments
{
    public class GetUserDocumentsQuery : IRequest<IList<DocumentListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public DocumentTarget Target { get; set; }
        public int OwnerId { get; set; }
        public int? DocumentDefinitionId { get; set; }
    }
}
