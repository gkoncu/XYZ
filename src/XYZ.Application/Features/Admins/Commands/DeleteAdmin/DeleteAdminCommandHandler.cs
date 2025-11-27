using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Admins.Commands.DeleteAdmin
{
    public sealed class DeleteAdminCommandHandler
        : IRequestHandler<DeleteAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public DeleteAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(
            DeleteAdminCommand request,
            CancellationToken cancellationToken)
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

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync(cancellationToken);

            return admin.Id;
        }
    }
}
