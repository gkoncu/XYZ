using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Queries.GetAllCoaches
{
    public class GetAllCoachesQueryValidator : AbstractValidator<GetAllCoachesQuery>
    {
        private static readonly string[] AllowedSortBy = new[]
        {
            "Id", "FirstName", "LastName", "FullName", "Email", "BranchName", "ClassesCount", "CreatedAt"
        };

        public GetAllCoachesQueryValidator()
        {
            RuleFor(x => x.SearchTerm).MaximumLength(200);

            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 200);

            RuleFor(x => x.SortBy)
                .Must(v => v == null || AllowedSortBy.Contains(v))
                .WithMessage($"SortBy sadece şu değerlerden biri olabilir: {string.Join(", ", AllowedSortBy)}");

            RuleFor(x => x.SortDir)
                .Must(v => v == null || v.ToLower() == "asc" || v.ToLower() == "desc")
                .WithMessage("SortDir 'asc' veya 'desc' olmalıdır.");
        }
    }
}
