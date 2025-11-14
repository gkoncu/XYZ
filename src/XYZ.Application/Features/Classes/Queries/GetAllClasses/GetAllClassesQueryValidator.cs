using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesQueryValidator : AbstractValidator<GetAllClassesQuery>
    {
        private static readonly string[] AllowedSortBy = new[]
        {
            "Id", "Name", "BranchName", "StudentsCount", "CoachesCount", "CreatedAt"
        };

        public GetAllClassesQueryValidator()
        {
            RuleFor(x => x.SearchTerm).MaximumLength(200);
            RuleFor(x => x.BranchId).GreaterThan(0).When(x => x.BranchId.HasValue);

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
