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
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Invalid student id.");

            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
                .WithMessage("Start date cannot bigger than end date.");
        }
    }
}
