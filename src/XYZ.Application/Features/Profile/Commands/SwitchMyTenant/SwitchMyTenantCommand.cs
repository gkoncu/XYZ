using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.SwitchMyTenant;

public sealed class SwitchMyTenantCommand : IRequest, IRequirePermission
{
    public int TenantId { get; init; }

    public string PermissionKey => PermissionNames.Tenants.Switch;
    public PermissionScope? MinimumScope => PermissionScope.AllTenants;
}
