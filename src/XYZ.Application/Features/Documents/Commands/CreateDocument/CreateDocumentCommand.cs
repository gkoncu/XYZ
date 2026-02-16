using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Documents.Upload;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int DocumentDefinitionId { get; set; }

        public int? StudentId { get; set; }
        public int? CoachId { get; set; }

        public string? Name { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
