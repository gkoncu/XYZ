using MediatR;
using System.Collections.Generic;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance
{
    public sealed class UpdateSessionAttendanceCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Attendance.Take;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int SessionId { get; set; }

        public IList<UpdateSessionAttendanceItem> Items { get; set; }
            = new List<UpdateSessionAttendanceItem>();
    }

    public sealed class UpdateSessionAttendanceItem
    {
        public int AttendanceId { get; set; }

        public AttendanceStatus Status { get; set; }
        public string? Note { get; set; }
        public int? Score { get; set; }
        public string? CoachComment { get; set; }
    }
}
