using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services.Auth
{
    public sealed class UserLookupService : IUserLookup
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IApplicationDbContext _context;

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
            var user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phone);
            return await ToIdentityOrNullAsync(user, ct);
        }

        public async Task<UserIdentity?> FindByIdentifierAsync(string identifier, CancellationToken ct = default)
        {
            var user = await _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.CoachProfile)
            .Include(u => u.AdminProfile)
            .FirstOrDefaultAsync(x => x.Email == identifier || x.UserName == identifier, ct);


            if (user is null) return null;


            var roles = await _userManager.GetRolesAsync(user);


            return new UserIdentity(
                user.Id,
                user.Email,
                user.PhoneNumber,
                roles.ToArray(),
                user.TenantId.ToString(),
                user.StudentId,
                user.CoachId,
                user.AdminId
            );
        }

        private async Task<UserIdentity?> ToIdentityOrNullAsync(ApplicationUser? user, CancellationToken ct)
        {
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            string? tenantId = user.TenantId.ToString();

            return new UserIdentity(
                user.Id,
                user.Email,
                user.PhoneNumber,
                roles.ToArray(),
                user.TenantId.ToString(),
                user.StudentId,
                user.CoachId,
                user.AdminId
            );
        }

        public async Task<UserIdentity?> FindByUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var user = await _context.Users
                .Include(u => u.StudentProfile)
                .Include(u => u.CoachProfile)
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(x => x.Id == userId, ct);

            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserIdentity(
                user.Id,
                user.Email,
                user.PhoneNumber,
                roles.ToArray(),
                user.TenantId.ToString(),
                user.StudentId,
                user.CoachId,
                user.AdminId
            );
        }
    }
}
