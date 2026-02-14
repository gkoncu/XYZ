using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.DeleteDocumentDefinition;

public sealed class DeleteDocumentDefinitionCommand : IRequest<int>, IRequirePermission
{
    public int Id { get; set; }

    public string PermissionKey => PermissionNames.Documents.DefinitionsManage;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;
}