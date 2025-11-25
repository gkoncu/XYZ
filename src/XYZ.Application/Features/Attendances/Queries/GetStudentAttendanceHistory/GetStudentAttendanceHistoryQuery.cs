using MediatR;
using System;
using System.Collections.Generic;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQuery
        : IRequest<IList<StudentAttendanceHistoryItemDto>>
    {
        public int StudentId { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}
