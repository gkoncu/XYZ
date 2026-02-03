using FluentValidation;

namespace XYZ.Application.Features.Profile.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("CurrentPassword is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required.")
            .Must((m, p) => !string.Equals(m.CurrentPassword, p, StringComparison.Ordinal))
            .WithMessage("NewPassword must be different from CurrentPassword.");
    }
}
