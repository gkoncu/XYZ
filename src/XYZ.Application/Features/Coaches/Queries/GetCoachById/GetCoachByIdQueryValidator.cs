using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Queries.GetCoachById
{
    public class GetCoachByIdQueryValidator : AbstractValidator<GetCoachByIdQuery>
    {
        public GetCoachByIdQueryValidator()
        {
            RuleFor(x => x.CoachId).GreaterThan(0);
        }
    }
}
