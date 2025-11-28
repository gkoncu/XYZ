using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Admins.Queries.GetAdminById
{
    public sealed class GetAdminByIdQueryHandler
        : IRequestHandler<GetAdminByIdQuery, AdminDetailDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetAdminByIdQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<AdminDetailDto?> Handle(
            GetAdminByIdQuery request,
            CancellationToken cancellationToken)
        {
            var role = _current.Role;
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
                    if (tenantId.HasValue)
                    {
                        q = q.Where(a => a.TenantId == tenantId.Value);
                    }
                    else
                    {
                        return null;
                    }
                    break;

                default:
                    return null;
            }

            var admin = await q
                .Where(a => a.Id == request.AdminId)
                .FirstOrDefaultAsync(cancellationToken);

            if (admin is null)
            {
                return null;
            }

            return new AdminDetailDto
            {
                Id = admin.Id,
                UserId = admin.UserId,
                FullName = admin.User.FullName,
                Email = admin.User.Email ?? string.Empty,
                PhoneNumber = admin.User.PhoneNumber,
                TenantId = admin.TenantId,
                TenantName = admin.Tenant.Name,
                IdentityNumber = admin.IdentityNumber,
                CanManageUsers = admin.CanManageUsers,
                CanManageFinance = admin.CanManageFinance,
                CanManageSettings = admin.CanManageSettings,
                IsActive = admin.IsActive && admin.User.IsActive,
                CreatedAt = admin.CreatedAt,
                UpdatedAt = admin.UpdatedAt
            };
        }
    }
}
