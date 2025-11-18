using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommandValidator
        : AbstractValidator<UnassignStudentFromClassCommand>
    {
        public UnassignStudentFromClassCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
            RuleFor(x => x.ClassId).GreaterThan(0);
        }
    }
}
