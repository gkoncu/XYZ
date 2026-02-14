using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
    }
}
