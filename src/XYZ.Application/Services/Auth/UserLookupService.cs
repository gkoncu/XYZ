using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services.Auth
{
    public sealed class UserLookupService : IUserLookup
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _context;

        public UserLookupService(UserManager<ApplicationUser> userManager, IApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<UserIdentity?> FindByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await ToIdentityOrNullAsync(user, ct);
        }

        public async Task<UserIdentity?> FindByPhoneAsync(string phone, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phone, ct);

            return await ToIdentityOrNullAsync(user, ct);
        }

        public async Task<UserIdentity?> FindByIdentifierAsync(string identifier, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == identifier || x.UserName == identifier, ct);

            return await ToIdentityOrNullAsync(user, ct);
        }

        public async Task<UserIdentity?> FindByUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, ct);

            return await ToIdentityOrNullAsync(user, ct);
        }

        private async Task<UserIdentity?> ToIdentityOrNullAsync(ApplicationUser? user, CancellationToken ct)
        {
            if (user is null)
                return null;

            if (!user.IsActive)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var isSuperAdmin = roles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));

            if (!isSuperAdmin)
            {
                var tenantActive = await _context.Tenants
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == user.TenantId, ct);

                if (!tenantActive)
                    return null;
            }

            var (studentId, coachId, adminId) = await FindProfileIdsAsync(user, ct);

            return new UserIdentity(
                user.Id,
                user.Email,
                user.PhoneNumber,
                roles.ToArray(),
                user.TenantId.ToString(),
                studentId,
                coachId,
                adminId
            );
        }

        private async Task<(string? StudentId, string? CoachId, string? AdminId)> FindProfileIdsAsync(
            ApplicationUser user,
            CancellationToken ct)
        {
            var tenantId = user.TenantId;

            var studentId = await _context.Students
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(s => s.IsActive && s.TenantId == tenantId && s.UserId == user.Id)
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct);

            var coachId = await _context.Coaches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.IsActive && c.TenantId == tenantId && c.UserId == user.Id)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(ct);

            var adminId = await _context.Admins
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(a => a.IsActive && a.TenantId == tenantId && a.UserId == user.Id)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync(ct);

            return (studentId?.ToString(), coachId?.ToString(), adminId?.ToString());
        }
    }
}
