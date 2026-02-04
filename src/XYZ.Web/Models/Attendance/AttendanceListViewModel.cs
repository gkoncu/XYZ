using XYZ.Application.Common.Models;
using XYZ.Application.Features.Attendances.Queries.GetAttendanceList;

namespace XYZ.Web.Models.Attendance
{
    public sealed class AttendanceListViewModel
    {
        public AttendanceListFilter Filter { get; set; } = new();

        public PaginationResult<AttendanceListItemDto> Result { get; set; } = new()
        {
            Items = new List<AttendanceListItemDto>(),
            PageNumber = 1,
            PageSize = 50,
            TotalCount = 0
        };

        public bool IsStudentHistoryMode => Filter.StudentId.HasValue;
    }

    public sealed class AttendanceListFilter
    {
        public int? StudentId { get; set; }
        public int? ClassId { get; set; }

        public string? From { get; set; }
        public string? To { get; set; }

        public int? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
