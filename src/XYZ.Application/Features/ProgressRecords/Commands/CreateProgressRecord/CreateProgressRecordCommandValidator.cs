using FluentValidation;

namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class CreateProgressRecordCommandValidator : AbstractValidator<CreateProgressRecordCommand>
    {
        public CreateProgressRecordCommandValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Student selection is mandatory.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Branch selection is mandatory.");

            RuleFor(x => x.RecordDate)
                .NotEmpty().WithMessage("Record date is mandatory.");

            RuleForEach(x => x.Values).ChildRules(v =>
            {
                v.RuleFor(x => x.ProgressMetricDefinitionId)
                    .GreaterThan(0).WithMessage("Invalid metric.");

                v.RuleFor(x => x)
                    .Must(HasExactlyOneValue)
                    .WithMessage("Only one value for every metric (Decimal / Number / Text).");
            });
        }

        private static bool HasExactlyOneValue(MetricValueInput v)
        {
            var filled =
                (v.DecimalValue.HasValue ? 1 : 0) +
                (v.IntValue.HasValue ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(v.TextValue) ? 1 : 0);

            return filled <= 1;
        }
    }
}
