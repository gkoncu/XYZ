using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Services.Auth
{
    public sealed class PasswordSignInService : IPasswordSignIn
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PasswordSignInService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PasswordSignInResult> CheckAsync(string userId, string password, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new PasswordSignInResult(PasswordSignInStatus.InvalidCredentials);

            var ok = await _userManager.CheckPasswordAsync(user, password);
            if (!ok)
                return new PasswordSignInResult(PasswordSignInStatus.InvalidCredentials);

            return new PasswordSignInResult(PasswordSignInStatus.Success);
        }
    }
}
