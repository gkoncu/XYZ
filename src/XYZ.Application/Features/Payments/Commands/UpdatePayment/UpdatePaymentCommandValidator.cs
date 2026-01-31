using FluentValidation;

namespace XYZ.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommandValidator
        : AbstractValidator<UpdatePaymentCommand>
    {
        public UpdatePaymentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(99999);

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DiscountAmount.HasValue);

            RuleFor(x => x)
                .Must(x => !x.DiscountAmount.HasValue || x.DiscountAmount.Value <= x.Amount)
                .WithMessage("DiscountAmount cannot be greater than Amount.");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
