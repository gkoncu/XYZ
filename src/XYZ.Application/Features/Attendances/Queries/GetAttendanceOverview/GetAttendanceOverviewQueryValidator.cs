using FluentValidation;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview
{
    public class GetAttendanceOverviewQueryValidator
        : AbstractValidator<GetAttendanceOverviewQuery>
    {
        public GetAttendanceOverviewQueryValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0);

            RuleFor(x => x.From)
                .NotEmpty();

            RuleFor(x => x.To)
                .NotEmpty();

            RuleFor(x => x)
                .Must(x => x.From <= x.To)
                .WithMessage("From, To tarihinden büyük olamaz.");
        }
    }
}
