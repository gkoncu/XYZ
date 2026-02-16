using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetSessionAttendance
{
    public class GetSessionAttendanceQuery : IRequest<SessionAttendanceDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Attendance.Read;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int ClassSessionId { get; set; }
    }
}
