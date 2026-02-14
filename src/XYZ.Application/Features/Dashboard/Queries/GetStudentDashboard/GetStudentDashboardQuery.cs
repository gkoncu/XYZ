using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;

public sealed class GetStudentDashboardQuery : IRequest<StudentDashboardDto>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Students.Read;
    public PermissionScope? MinimumScope => PermissionScope.Self;
}