using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsQueryValidator : AbstractValidator<GetAllStudentsQuery>
    {
        public GetAllStudentsQueryValidator()
        {
            RuleFor(x => x.SearchTerm).MaximumLength(200);
            RuleFor(x => x.ClassId).GreaterThan(0).When(x => x.ClassId.HasValue);
        }
    }
}
