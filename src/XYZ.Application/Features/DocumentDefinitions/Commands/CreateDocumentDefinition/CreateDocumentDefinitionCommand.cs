using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition;

public sealed class CreateDocumentDefinitionCommand : IRequest<int>, IRequirePermission
{
    public DocumentTarget Target { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public string PermissionKey => PermissionNames.Documents.DefinitionsManage;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;
}