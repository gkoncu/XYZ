using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UnassignCoachToClass
{
    public class UnassignCoachFromClassCommandValidator : AbstractValidator<UnassignCoachFromClassCommand>
    {
        public UnassignCoachFromClassCommandValidator()
        {
            RuleFor(x => x.CoachId)
                .GreaterThan(0);

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue);
        }
    }
}
