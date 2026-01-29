using FluentValidation;

namespace XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance
{
    public sealed class UpdateSessionAttendanceCommandValidator
        : AbstractValidator<UpdateSessionAttendanceCommand>
    {
        public UpdateSessionAttendanceCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0);

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Attendance items cannot be null.")
                .NotEmpty()
                .WithMessage("Attendance items cannot be empty.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.AttendanceId)
                    .GreaterThan(0);

                item.RuleFor(i => i.Status)
                    .IsInEnum();

                item.RuleFor(i => i.Score)
                    .InclusiveBetween(0, 100)
                    .When(i => i.Score.HasValue);

                item.RuleFor(i => i.Note)
                    .MaximumLength(1000)
                    .When(i => !string.IsNullOrWhiteSpace(i.Note))
                    .WithMessage("Note must be at most 1000 characters.");

                item.RuleFor(i => i.CoachComment)
                    .MaximumLength(2000)
                    .When(i => !string.IsNullOrWhiteSpace(i.CoachComment))
                    .WithMessage("CoachComment must be at most 2000 characters.");
            });
        }
    }
}
