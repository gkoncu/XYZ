using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQuery
        : IRequest<IList<StudentAttendanceHistoryItemDto>>
    {
        public int StudentId { get; set; }
    }
}
