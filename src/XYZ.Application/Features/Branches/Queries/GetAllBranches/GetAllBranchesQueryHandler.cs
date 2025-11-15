using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Branches.Queries.GetAllBranches
{
    public class GetAllBranchesQueryHandler
        : IRequestHandler<GetAllBranchesQuery, PaginationResult<BranchListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetAllBranchesQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PaginationResult<BranchListItemDto>> Handle(
            GetAllBranchesQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var query = _context.Branches
                .AsNoTracking()
                .Where(b => b.TenantId == tenantId);

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
