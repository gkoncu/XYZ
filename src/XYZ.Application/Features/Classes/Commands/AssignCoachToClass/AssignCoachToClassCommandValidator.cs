using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.AssignCoachToClass
{
    public class AssignCoachToClassCommandValidator : AbstractValidator<AssignCoachToClassCommand>
    {
        public AssignCoachToClassCommandValidator()
        {
            RuleFor(x => x.ClassId).GreaterThan(0);
            RuleFor(x => x.CoachId).GreaterThan(0);
        }
    }
}
