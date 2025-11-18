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

            RuleForEach(x => new[] { x.TechnicalScore, x.TacticalScore, x.PhysicalScore, x.MentalScore })
                .InclusiveBetween(0, 100)
                .When(x => x.HasValue());
        }
    }

    internal static class IntExtensions2
    {
        public static bool HasValue(this int? value) => value.HasValue;
    }
}
