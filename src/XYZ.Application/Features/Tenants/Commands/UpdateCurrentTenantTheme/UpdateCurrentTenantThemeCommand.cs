using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;

public sealed class UpdateCurrentTenantThemeCommand : IRequest, IRequirePermission
{
    public string PrimaryColor { get; set; } = "#3B82F6";
    public string SecondaryColor { get; set; } = "#1E40AF";
    public string? LogoUrl { get; set; }

    public string PermissionKey => PermissionNames.Settings.TenantSettingsManage;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;
}
