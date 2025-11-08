using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services.Auth
{
    public sealed class UserLookupService : IUserLookup
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserLookupService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
            if (identifier.Contains("@"))
                return await FindByEmailAsync(identifier, ct);

            return await FindByPhoneAsync(identifier, ct);
        }

        private async Task<UserIdentity?> ToIdentityOrNullAsync(ApplicationUser? user, CancellationToken ct)
        {
            if (user is null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            string? tenantId = user.TenantId.ToString();

            return new UserIdentity(
                UserId: user.Id,
                Email: user.Email,
                PhoneNumber: user.PhoneNumber,
                Roles: roles as IReadOnlyCollection<string> ?? roles.ToList(),
                TenantId: tenantId
            );
        }
    }
}
