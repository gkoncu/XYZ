using MediatR;
using System;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview
{
    public class GetAttendanceOverviewQuery
        : IRequest<AttendanceOverviewDto>
    {
        public int ClassId { get; set; }

        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
    }
}
