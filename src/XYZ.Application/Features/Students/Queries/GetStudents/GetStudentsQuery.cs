using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Queries.GetStudents
{
    public class GetStudentsQuery : IRequest<StudentListResponse>
    {
        public bool? IsActive { get; set; }
        public int? ClassId { get; set; }
        public string? Branch { get; set; }

        public string? SearchTerm { get; set; }
        public string? Name { get; set; }
        public string? ParentName { get; set; }
        public string? PhoneNumber { get; set; }

        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? BirthDateFrom { get; set; }
        public DateTime? BirthDateTo { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string SortBy { get; set; } = "LastName";
        public bool SortDescending { get; set; } = false;
    }

    public class StudentListResponse
    {
        public List<StudentListDto> Students { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class GetStudentsQueryValidator : AbstractValidator<GetStudentsQuery>
    {
        public GetStudentsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

            When(x => x.ClassId.HasValue, () =>
            {
                RuleFor(x => x.ClassId)
                    .GreaterThan(0).WithMessage("Class ID must be greater than 0");
            });

            When(x => !string.IsNullOrEmpty(x.Branch), () =>
            {
                RuleFor(x => x.Branch)
                    .MaximumLength(50).WithMessage("Branch cannot exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.SearchTerm), () =>
            {
                RuleFor(x => x.SearchTerm)
                    .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
                    .MinimumLength(2).WithMessage("Search term must be at least 2 characters");
            });

            When(x => !string.IsNullOrEmpty(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .MaximumLength(50).WithMessage("Name cannot exceed 50 characters")
                    .MinimumLength(2).WithMessage("Name must be at least 2 characters");
            });

            When(x => !string.IsNullOrEmpty(x.ParentName), () =>
            {
                RuleFor(x => x.ParentName)
                    .MaximumLength(100).WithMessage("Parent name cannot exceed 100 characters")
                    .MinimumLength(2).WithMessage("Parent name must be at least 2 characters");
            });

            When(x => !string.IsNullOrEmpty(x.PhoneNumber), () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
                    .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Phone number contains invalid characters");
            });

            When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue, () =>
            {
                RuleFor(x => x.CreatedBefore)
                    .GreaterThan(x => x.CreatedAfter).WithMessage("CreatedBefore must be after CreatedAfter");
            });

            When(x => x.BirthDateFrom.HasValue && x.BirthDateTo.HasValue, () =>
            {
                RuleFor(x => x.BirthDateTo)
                    .GreaterThan(x => x.BirthDateFrom).WithMessage("BirthDateTo must be after BirthDateFrom");
            });

            RuleFor(x => x.SortBy)
                .Must(BeAValidSortProperty)
                .WithMessage("SortBy must be a valid property: LastName, FirstName, Email, CreatedAt, BirthDate");
        }

        private static bool BeAValidSortProperty(string sortBy)
        {
            var validProperties = new[] { "LastName", "FirstName", "Email", "CreatedAt", "BirthDate", "ClassName" };
            return validProperties.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
        }
    }
}
