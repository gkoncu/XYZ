using MediatR;
using System;
using XYZ.Application.Common.Models;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public class GetAttendanceListQuery
        : IRequest<PaginationResult<AttendanceListItemDto>>
    {
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
