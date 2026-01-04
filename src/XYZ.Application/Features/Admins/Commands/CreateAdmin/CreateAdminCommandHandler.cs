using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Admins.Commands.CreateAdmin
{
    public sealed class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _current = currentUser;
            _userManager = userManager;
        }

        public async Task<int> Handle(CreateAdminCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Admin oluşturma yetkiniz yok.");

            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new InvalidOperationException("UserId zorunludur.");

            var tenantId = _current.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
                throw new InvalidOperationException("Kullanıcı bulunamadı.");

            if (user.TenantId != tenantId)
                throw new UnauthorizedAccessException("Bu kullanıcı farklı bir tenant'a ait.");

            var exists = await _context.Admins.AnyAsync(a => a.UserId == user.Id, ct);
            if (exists)
                throw new InvalidOperationException("Bu kullanıcı için zaten Admin profili oluşturulmuş.");

            if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                var identityInTenant = await _context.Admins
                    .AnyAsync(a => a.TenantId == tenantId && a.IdentityNumber == request.IdentityNumber, ct);

                if (identityInTenant)
                    throw new InvalidOperationException("TC Kimlik No bu tenant içinde zaten kullanılıyor.");
            }

            var admin = new Admin
            {
                UserId = user.Id,
                TenantId = tenantId,
                IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : request.IdentityNumber.Trim(),
                CanManageUsers = request.CanManageUsers,
                CanManageFinance = request.CanManageFinance,
                CanManageSettings = request.CanManageSettings,
                IsActive = true
            };

            await _context.Admins.AddAsync(admin, ct);
            await _context.SaveChangesAsync(ct);

            return admin.Id;
        }
    }
}

