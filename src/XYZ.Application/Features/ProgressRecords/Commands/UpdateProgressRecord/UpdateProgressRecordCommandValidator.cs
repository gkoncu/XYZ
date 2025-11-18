using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class UpdateProgressRecordCommandValidator
        : AbstractValidator<UpdateProgressRecordCommand>
    {
        public UpdateProgressRecordCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.RecordDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .When(x => x.RecordDate != default);

            RuleFor(x => x.Height)
                .GreaterThan(0).When(x => x.Height.HasValue);

            RuleFor(x => x.Weight)
                .GreaterThan(0).When(x => x.Weight.HasValue);

            RuleFor(x => x.BodyFatPercentage)
                .InclusiveBetween(0, 100).When(x => x.BodyFatPercentage.HasValue);

            RuleFor(x => x.VerticalJump)
                .GreaterThanOrEqualTo(0).When(x => x.VerticalJump.HasValue);

            RuleFor(x => x.SprintTime)
                .GreaterThanOrEqualTo(0).When(x => x.SprintTime.HasValue);

            RuleFor(x => x.Endurance)
                .GreaterThanOrEqualTo(0).When(x => x.Endurance.HasValue);

            RuleFor(x => x.Flexibility)
                .GreaterThanOrEqualTo(0).When(x => x.Flexibility.HasValue);

            RuleFor(x => x.TechnicalScore)
                .InclusiveBetween(0, 100).When(x => x.TechnicalScore.HasValue);

            RuleFor(x => x.TacticalScore)
                .InclusiveBetween(0, 100).When(x => x.TacticalScore.HasValue);

            RuleFor(x => x.PhysicalScore)
                .InclusiveBetween(0, 100).When(x => x.PhysicalScore.HasValue);

            RuleFor(x => x.MentalScore)
                .InclusiveBetween(0, 100).When(x => x.MentalScore.HasValue);
        }
    }
}
