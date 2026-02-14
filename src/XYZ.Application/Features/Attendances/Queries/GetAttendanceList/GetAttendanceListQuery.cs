using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public class GetAttendanceListQuery
        : IRequest<PaginationResult<AttendanceListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Attendance.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int? StudentId { get; set; }
        public int? ClassId { get; set; }
        public int? ClassSessionId { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public AttendanceStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
