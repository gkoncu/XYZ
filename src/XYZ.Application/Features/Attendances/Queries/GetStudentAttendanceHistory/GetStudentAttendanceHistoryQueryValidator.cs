using FluentValidation;

namespace XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory
{
    public class GetStudentAttendanceHistoryQueryValidator
        : AbstractValidator<GetStudentAttendanceHistoryQuery>
    {
        public GetStudentAttendanceHistoryQueryValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0);

            RuleFor(x => x)
                .Must(x =>
                    !x.From.HasValue ||
                    !x.To.HasValue ||
                    x.From.Value <= x.To.Value)
                .WithMessage("From, To tarihinden büyük olamaz.");
        }
    }
}
