using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class GetStudentProgressRecordsQueryValidator
        : AbstractValidator<GetStudentProgressRecordsQuery>
    {
        public GetStudentProgressRecordsQueryValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
