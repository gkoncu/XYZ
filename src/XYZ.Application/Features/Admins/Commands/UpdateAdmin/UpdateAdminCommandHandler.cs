using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommandHandler
        : IRequestHandler<UpdateAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UpdateAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(UpdateAdminCommand request, CancellationToken cancellationToken)
        {
            var role = _current.Role;
            var tenantId = _current.TenantId;

            var q = _context.Admins.AsQueryable();

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
                        throw new UnauthorizedAccessException("Tenant bilgisi bulunamadı.");
                    }
                    break;

                default:
                    throw new UnauthorizedAccessException("Bu işlemi yapmaya yetkiniz yok.");
            }

            var admin = await q
                .FirstOrDefaultAsync(a => a.Id == request.AdminId, cancellationToken);

            if (admin is null)
            {
                throw new KeyNotFoundException($"Admin bulunamadı. Id={request.AdminId}");
            }

            admin.IdentityNumber = request.IdentityNumber;
            admin.CanManageUsers = request.CanManageUsers;
            admin.CanManageFinance = request.CanManageFinance;
            admin.CanManageSettings = request.CanManageSettings;

            await _context.SaveChangesAsync(cancellationToken);

            return admin.Id;
        }
    }
}
