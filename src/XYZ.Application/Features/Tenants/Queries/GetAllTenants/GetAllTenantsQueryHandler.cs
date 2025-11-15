using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Tenants.Queries.GetAllTenants
{
    public class GetAllTenantsQueryHandler
        : IRequestHandler<GetAllTenantsQuery, PaginationResult<TenantListItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllTenantsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResult<TenantListItemDto>> Handle(
            GetAllTenantsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _context.Tenants
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(term) ||
                    t.Subdomain.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var sortBy = (request.SortBy ?? "name").ToLowerInvariant();
            var sortDir = (request.SortDirection ?? "asc").ToLowerInvariant();

            query = (sortBy, sortDir) switch
            {
                ("subdomain", "desc") => query.OrderByDescending(t => t.Subdomain),
                ("subdomain", _) => query.OrderBy(t => t.Subdomain),

                ("createdat", "desc") => query.OrderByDescending(t => t.CreatedAt),
                ("createdat", _) => query.OrderBy(t => t.CreatedAt),

                ("name", "desc") => query.OrderByDescending(t => t.Name),
                ("name", _) => query.OrderBy(t => t.Name),

                _ => query.OrderBy(t => t.Name)
            };

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TenantListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Subdomain = t.Subdomain,
                    IsActive = t.IsActive,
                    SubscriptionPlan = t.SubscriptionPlan,
                    SubscriptionStartDate = t.SubscriptionStartDate,
                    SubscriptionEndDate = t.SubscriptionEndDate,
                    ActiveBranchCount = t.Branches.Count(b => b.IsActive),
                    ActiveClassCount = t.Classes.Count(c => c.IsActive),
                    ActiveStudentCount = t.Students.Count(s => s.IsActive),
                    ActiveCoachCount = t.Coaches.Count(c => c.IsActive)
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<TenantListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
