using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Branches.Queries.GetAllBranches
{
    public class GetAllBranchesQueryHandler
        : IRequestHandler<GetAllBranchesQuery, PaginationResult<BranchListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllBranchesQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<BranchListItemDto>> Handle(
            GetAllBranchesQuery request,
            CancellationToken cancellationToken)
        {
            var query = _dataScope.Branches()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var sortBy = (request.SortBy ?? "name").ToLowerInvariant();
            var sortDir = (request.SortDirection ?? "asc").ToLowerInvariant();

            query = (sortBy, sortDir) switch
            {
                ("createdat", "desc") => query.OrderByDescending(b => b.CreatedAt),
                ("createdat", _) => query.OrderBy(b => b.CreatedAt),

                ("name", "desc") => query.OrderByDescending(b => b.Name),
                ("name", _) => query.OrderBy(b => b.Name),

                _ => query.OrderBy(b => b.Name)
            };

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BranchListItemDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    TenantId = b.TenantId,
                    TenantName = b.Tenant.Name,
                    ClassCount = b.Classes.Count(c => c.IsActive)
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<BranchListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
