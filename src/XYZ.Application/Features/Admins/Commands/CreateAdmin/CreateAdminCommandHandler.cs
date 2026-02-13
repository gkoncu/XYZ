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
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new InvalidOperationException("UserId zorunludur.");

            var tenantId = _current.TenantId;
            if (!tenantId.HasValue || tenantId.Value <= 0)
                throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var effectiveTenantId = tenantId.Value;

            if (request.TenantId.HasValue && request.TenantId.Value != effectiveTenantId)
                throw new UnauthorizedAccessException("Farklı tenant için admin oluşturamazsınız.");

            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
                throw new InvalidOperationException("Kullanıcı bulunamadı.");

            if (user.TenantId != effectiveTenantId)
                throw new UnauthorizedAccessException("Bu kullanıcı farklı bir tenant'a ait.");

            var exists = await _context.Admins.AnyAsync(a => a.UserId == user.Id, ct);
            if (exists)
                throw new InvalidOperationException("Bu kullanıcı için zaten Admin profili oluşturulmuş.");

            var identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
                ? null
                : request.IdentityNumber.Trim();

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var identityInTenant = await _context.Admins
                    .AnyAsync(a => a.IdentityNumber == identityNumber, ct);

                if (identityInTenant)
                    throw new InvalidOperationException("TC Kimlik No bu tenant içinde zaten kullanılıyor.");
            }

            var admin = new Admin
            {
                UserId = user.Id,
                TenantId = effectiveTenantId,
                IdentityNumber = identityNumber,
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
