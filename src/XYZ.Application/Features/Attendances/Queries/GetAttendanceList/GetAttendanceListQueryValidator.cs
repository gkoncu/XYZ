using FluentValidation;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public sealed class GetAttendanceListQueryValidator
        : AbstractValidator<GetAttendanceListQuery>
    {
        public GetAttendanceListQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200)
                .WithMessage("PageSize must be between 1 and 200.");

            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
                .WithMessage("From cannot be greater than To.");

            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .When(x => x.StudentId.HasValue)
                .WithMessage("StudentId must be greater than 0.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue)
                .WithMessage("ClassId must be greater than 0.");

            RuleFor(x => x.ClassSessionId)
                .GreaterThan(0)
                .When(x => x.ClassSessionId.HasValue)
                .WithMessage("ClassSessionId must be greater than 0.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Status is not a valid value.");
        }
    }
}
