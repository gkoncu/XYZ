using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Attendances.Queries.GetSessionAttendance
{
    public class GetSessionAttendanceQueryValidator
        : AbstractValidator<GetSessionAttendanceQuery>
    {
        public GetSessionAttendanceQueryValidator()
        {
            RuleFor(x => x.ClassSessionId).GreaterThan(0);
        }
    }
}
