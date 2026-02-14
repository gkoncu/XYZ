using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Queries.GetAllCoaches
{
    public class GetAllCoachesQuery : IRequest<PaginationResult<CoachListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Coaches.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; } = "FullName";
        public string? SortDir { get; set; } = "asc";
    }
}
