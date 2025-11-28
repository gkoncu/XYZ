using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Admins.Commands.CreateAdmin
{
    public sealed class CreateAdminCommandHandler
        : IRequestHandler<CreateAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public CreateAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(
            CreateAdminCommand request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException("UserId zorunludur.");
            }

            var role = _current.Role;
            var currentTenantId = _current.TenantId;

            int targetTenantId;

            switch (role)
            {
                case "SuperAdmin":
                    if (!request.TenantId.HasValue)
                    {
                        throw new InvalidOperationException(
                            "SuperAdmin için Kulüp (TenantId) zorunludur.");
                    }
                    targetTenantId = request.TenantId.Value;
                    break;

                case "Admin":
                    if (!currentTenantId.HasValue)
                    {
                        throw new UnauthorizedAccessException("Kulüp bilgisi bulunamadı.");
                    }
                    targetTenantId = currentTenantId.Value;
                    break;

                default:
                    throw new UnauthorizedAccessException(
                        "Admin oluşturma yetkiniz yok.");
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == request.UserId, cancellationToken);

            if (!userExists)
            {
                throw new KeyNotFoundException("Belirtilen kullanıcı bulunamadı.");
            }

            var alreadyExists = await _context.Admins
                .AnyAsync(a => a.UserId == request.UserId &&
                               a.TenantId == targetTenantId,
                          cancellationToken);

            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    "Bu kullanıcı belirtilen kulüp için zaten admin olarak tanımlanmış.");
            }

            var admin = new Admin
            {
                UserId = request.UserId,
                TenantId = targetTenantId,
                IdentityNumber = request.IdentityNumber,
                CanManageUsers = request.CanManageUsers,
                CanManageFinance = request.CanManageFinance,
                CanManageSettings = request.CanManageSettings,
                IsActive = true
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync(cancellationToken);

            return admin.Id;
        }
    }
}
