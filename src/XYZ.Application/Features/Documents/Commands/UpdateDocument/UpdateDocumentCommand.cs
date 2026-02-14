using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Upload;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FilePath { get; set; }
        public bool? IsActive { get; set; }
    }
}
