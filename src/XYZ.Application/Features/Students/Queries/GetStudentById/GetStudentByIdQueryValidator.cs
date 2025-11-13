using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Queries.GetStudentById
{
    public class GetStudentByIdQueryValidator : AbstractValidator<GetStudentByIdQuery>
    {
        public GetStudentByIdQueryValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
