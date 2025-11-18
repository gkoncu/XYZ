using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQueryValidator
        : AbstractValidator<GetStudentAttendanceHistoryQuery>
    {
        public GetStudentAttendanceHistoryQueryValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
