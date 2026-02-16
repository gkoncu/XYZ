using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;

public sealed class GetAdminCoachDashboardQuery : IRequest<AdminCoachDashboardDto>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Attendance.Read;
    public PermissionScope? MinimumScope => PermissionScope.OwnClasses;
}
