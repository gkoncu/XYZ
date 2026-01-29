using FluentValidation;
using System;
using System.Linq;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessions
{
    public class GetClassSessionsQueryValidator
        : AbstractValidator<GetClassSessionsQuery>
    {
        private static readonly string[] AllowedSortBy = new[]
        {
            "Id",
            "Date",
            "StartTime",
            "EndTime",
            "ClassName",
            "CreatedAt"
        };

        public GetClassSessionsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200);

            RuleFor(x => x.SortBy)
                .Must(v => v == null || AllowedSortBy.Contains(v))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

            RuleFor(x => x.SortDir)
                .Must(v => v == null ||
                           v.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                           v.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDir must be either 'asc' or 'desc'.");

            RuleFor(x => x)
                .Must(x =>
                    !x.From.HasValue ||
                    !x.To.HasValue ||
                    x.From.Value <= x.To.Value)
                .WithMessage("From cannot be greater than To.");
        }
    }
}
