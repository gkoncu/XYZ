using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesQuery : IRequest<PaginationResult<ClassListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.Read;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public string? SearchTerm { get; set; }
        public int? BranchId { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "Name";
        public string? SortDir { get; set; } = "asc";
    }
}
