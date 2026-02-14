using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQuery : IRequest<DocumentDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
    }
}
