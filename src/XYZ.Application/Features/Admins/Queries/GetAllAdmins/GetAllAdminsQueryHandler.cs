using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Admins.Queries.GetAllAdmins
{
    public sealed class GetAllAdminsQueryHandler
        : IRequestHandler<GetAllAdminsQuery, PaginationResult<AdminListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetAllAdminsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<PaginationResult<AdminListItemDto>> Handle(
            GetAllAdminsQuery request,
            CancellationToken cancellationToken)
        {
            var role = _current.Role ?? string.Empty;
            var tenantId = _current.TenantId;

            var q = _context.Admins
                .Include(a => a.User)
                .Include(a => a.Tenant)
                .AsQueryable();

            switch (role)
            {
                case "SuperAdmin":
                    break;

                case "Admin":
                    if (tenantId > 0)
                        q = q.Where(a => a.TenantId == tenantId);
                    else
                        q = q.Where(_ => false);
                    break;

                default:
                    q = q.Where(_ => false);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();

                q = q.Where(a =>
                    a.User.FirstName.ToLower().Contains(term)
                    || a.User.LastName.ToLower().Contains(term)
                    || (a.User.Email ?? "").ToLower().Contains(term)
                    || (a.IdentityNumber ?? "").ToLower().Contains(term)
                    || a.Tenant.Name.ToLower().Contains(term));
            }

            if (request.IsActive.HasValue)
            {
                var active = request.IsActive.Value;
                q = q.Where(a => a.IsActive == active && a.User.IsActive == active);
            }

            var page = request.PageNumber < 1 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;

            var totalCount = await q.CountAsync(cancellationToken);

            var items = await q
                .OrderBy(a => a.Tenant.Name)
                .ThenBy(a => a.User.LastName)
                .ThenBy(a => a.User.FirstName)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new AdminListItemDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    FullName = a.User.FullName,
                    Email = a.User.Email ?? string.Empty,
                    PhoneNumber = a.User.PhoneNumber,
                    TenantId = a.TenantId,
                    TenantName = a.Tenant.Name,
                    IdentityNumber = a.IdentityNumber,
                    CanManageUsers = a.CanManageUsers,
                    CanManageFinance = a.CanManageFinance,
                    CanManageSettings = a.CanManageSettings,
                    IsActive = a.IsActive && a.User.IsActive,
                    CreatedAt = a.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PaginationResult<AdminListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
