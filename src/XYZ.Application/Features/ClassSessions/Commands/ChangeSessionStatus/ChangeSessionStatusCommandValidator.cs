using FluentValidation;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.ChangeSessionStatus
{
    public class ChangeSessionStatusCommandValidator
        : AbstractValidator<ChangeSessionStatusCommand>
    {
        public ChangeSessionStatusCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0);

            RuleFor(x => x.Status)
                .IsInEnum();
        }
    }
}
