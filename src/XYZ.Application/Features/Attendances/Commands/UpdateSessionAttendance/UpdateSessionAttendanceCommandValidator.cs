using FluentValidation;

namespace XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance
{
    public class UpdateSessionAttendanceCommandValidator
        : AbstractValidator<UpdateSessionAttendanceCommand>
    {
        public UpdateSessionAttendanceCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0);

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Attendance listesi boş olamaz.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.AttendanceId)
                    .GreaterThan(0);

                item.RuleFor(i => i.Status)
                    .IsInEnum();

                item.RuleFor(i => i.Score)
                    .InclusiveBetween(0, 100)
                    .When(i => i.Score.HasValue);
            });
        }
    }
}
