using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitions;

public sealed class GetDocumentDefinitionsQuery : IRequest<IList<DocumentDefinitionListItemDto>>, IRequirePermission
{
    public DocumentTarget? Target { get; set; }
    public bool IncludeInactive { get; set; }

    public string PermissionKey => PermissionNames.Documents.DefinitionsManage;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;
}