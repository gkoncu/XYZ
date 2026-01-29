using FluentValidation;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesQueryValidator : AbstractValidator<GetAllClassesQuery>
    {
        private static readonly string[] AllowedSortBy =
        {
            "Id", "Name", "BranchName", "StudentsCount", "CoachesCount", "CreatedAt"
        };

        public GetAllClassesQueryValidator()
        {
            RuleFor(x => x.SearchTerm)
                .MaximumLength(50);

            RuleFor(x => x.BranchId)
                .GreaterThan(0)
                .When(x => x.BranchId.HasValue);

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200);

            RuleFor(x => x.SortBy)
                .Must(v => v == null || AllowedSortBy.Contains(v))
                .WithMessage($"SortBy must be one of: {string.Join(", ", AllowedSortBy)}");

            RuleFor(x => x.SortDir)
                .Must(v => v == null || v.Equals("asc", StringComparison.OrdinalIgnoreCase) || v.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDir must be either 'asc' or 'desc'.");
        }
    }
}
