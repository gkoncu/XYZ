using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Branches.Queries.GetAllBranches
{
    public sealed class GetAllBranchesQuery : IRequest<PaginationResult<BranchListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Branches.Read;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }
}
