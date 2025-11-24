using FluentValidation;
using System;

namespace XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession
{
    public class UpdateClassSessionCommandValidator
        : AbstractValidator<UpdateClassSessionCommand>
    {
        public UpdateClassSessionCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description != null);

            RuleFor(x => x.Location)
                .MaximumLength(500)
                .When(x => x.Location != null);

            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime)
                .WithMessage("EndTime, StartTime'dan büyük olmalıdır.");
        }
    }
}
