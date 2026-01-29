using FluentValidation;
using System;

namespace XYZ.Application.Features.ClassSessions.Commands.CreateClassSession
{
    public sealed class CreateClassSessionCommandValidator
        : AbstractValidator<CreateClassSessionCommand>
    {
        private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
        private static DateOnly MinDate => Today.AddYears(-1);
        private static DateOnly MaxDate => Today.AddDays(364);

        public CreateClassSessionCommandValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0);

            RuleFor(x => x.Date)
                .Must(d => d >= MinDate && d <= MaxDate)
                .WithMessage("Date must be within the last 1 year and the next 52 weeks.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Location)
                .MaximumLength(80)
                .When(x => !string.IsNullOrWhiteSpace(x.Location));

            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime)
                .WithMessage("EndTime must be greater than StartTime.");
        }
    }
}
