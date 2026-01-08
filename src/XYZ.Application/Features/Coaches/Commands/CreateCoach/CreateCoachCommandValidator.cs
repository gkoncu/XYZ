using FluentValidation;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public sealed class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
    {
        public CreateCoachCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.BranchId)
                .GreaterThan(0);

            When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber), () =>
            {
                RuleFor(x => x.IdentityNumber!)
                    .Length(11)
                    .Matches("^[0-9]{11}$")
                    .WithMessage("T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.LicenseNumber), () =>
            {
                RuleFor(x => x.LicenseNumber!)
                    .MaximumLength(50);
            });
        }
    }
}
