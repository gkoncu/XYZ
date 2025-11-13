using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.ActivateStudent
{
    public class ActivateStudentCommandValidator : AbstractValidator<ActivateStudentCommand>
    {
        public ActivateStudentCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
