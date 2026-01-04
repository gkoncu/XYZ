using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateCoachCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
            _userManager = userManager;
        }

        public async Task<int> Handle(CreateCoachCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Koç oluşturma yetkiniz yok.");

            var tenantId = _current.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var normalized = _userManager.NormalizeEmail(request.Email);
                var emailInTenant = await _context.Users
                    .AnyAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalized, ct);

                if (emailInTenant)
                    throw new InvalidOperationException("Bu e-posta adresi bu tenant içinde zaten kullanılıyor.");
            }

            if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                var identityInTenant = await _context.Coaches
                    .AnyAsync(c => c.TenantId == tenantId && c.IdentityNumber == request.IdentityNumber, ct);

                if (identityInTenant)
                    throw new InvalidOperationException("TC Kimlik No bu tenant içinde zaten kullanılıyor.");
            }

            if (_context is not DbContext dbContext)
                throw new InvalidOperationException("ApplicationDbContext transaction erişimi sağlanamadı.");

            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    BirthDate = request.BirthDate,
                    Gender = Enum.Parse<Gender>(request.Gender, true),
                    BloodType = Enum.Parse<BloodType>(request.BloodType, true),
                    TenantId = tenantId,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {msg}");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Coach");
                if (!roleResult.Succeeded)
                {
                    var msg = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Rol atanamadı (Coach): {msg}");
                }

                var coach = new Coach
                {
                    UserId = user.Id,
                    TenantId = tenantId,

                    BranchId = request.BranchId,
                    IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : request.IdentityNumber.Trim(),
                    LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim(),
                };

                await _context.Coaches.AddAsync(coach, ct);
                await _context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return coach.Id;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
