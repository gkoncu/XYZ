using FluentValidation;
using System;

namespace XYZ.Application.Features.Attendances.Queries.GetAttendanceList
{
    public class GetAttendanceListQueryValidator
        : AbstractValidator<GetAttendanceListQuery>
    {
        public GetAttendanceListQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber 0'dan büyük olmalıdır.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200)
                .WithMessage("PageSize 1 ile 200 arasında olmalıdır.");

            RuleFor(x => x)
                .Must(x =>
                    !x.From.HasValue ||
                    !x.To.HasValue ||
                    x.From.Value <= x.To.Value)
                .WithMessage("From, To tarihinden büyük olamaz.");

            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .When(x => x.StudentId.HasValue)
                .WithMessage("StudentId 0'dan büyük olmalıdır.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue)
                .WithMessage("ClassId 0'dan büyük olmalıdır.");

            RuleFor(x => x.ClassSessionId)
                .GreaterThan(0)
                .When(x => x.ClassSessionId.HasValue)
                .WithMessage("ClassSessionId 0'dan büyük olmalıdır.");
        }
    }
}
