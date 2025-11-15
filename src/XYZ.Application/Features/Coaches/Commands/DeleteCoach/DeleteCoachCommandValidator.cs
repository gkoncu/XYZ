using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach
{
    public class DeleteCoachCommandValidator : AbstractValidator<DeleteCoachCommand>
    {
        public DeleteCoachCommandValidator()
        {
            RuleFor(x => x.CoachId)
                .GreaterThan(0);
        }
    }
}
