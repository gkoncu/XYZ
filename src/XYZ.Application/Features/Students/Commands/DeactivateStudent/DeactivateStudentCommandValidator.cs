using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.DeactivateStudent
{
    public class DeactivateStudentCommandValidator : AbstractValidator<DeactivateStudentCommand>
    {
        public DeactivateStudentCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
