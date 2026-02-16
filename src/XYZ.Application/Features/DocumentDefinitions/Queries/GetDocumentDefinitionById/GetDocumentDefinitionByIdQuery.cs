using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById;

public sealed class GetDocumentDefinitionByIdQuery : IRequest<DocumentDefinitionDetailDto>, IRequirePermission
{
    public int Id { get; set; }

    public string PermissionKey => PermissionNames.Documents.DefinitionsManage;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;
}