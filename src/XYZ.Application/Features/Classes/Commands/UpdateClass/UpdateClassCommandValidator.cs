using FluentValidation;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
    {
        public UpdateClassCommandValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.BranchId)
                .GreaterThan(0);

            RuleFor(x => x.MaxCapacity)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.AgeGroupMin)
                .InclusiveBetween(0, 100)
                .When(x => x.AgeGroupMin.HasValue);

            RuleFor(x => x.AgeGroupMax)
                .InclusiveBetween(0, 100)
                .When(x => x.AgeGroupMax.HasValue);

            RuleFor(x => x)
                .Must(x =>
                    !x.AgeGroupMin.HasValue ||
                    !x.AgeGroupMax.HasValue ||
                    x.AgeGroupMin.Value <= x.AgeGroupMax.Value)
                .WithMessage("AgeGroupMin cannot be greater than AgeGroupMax.");
        }
    }
}
