using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;

public sealed class GetSuperAdminDashboardQuery : IRequest<SuperAdminDashboardDto>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Tenants.Read;
    public PermissionScope? MinimumScope => PermissionScope.AllTenants;
}