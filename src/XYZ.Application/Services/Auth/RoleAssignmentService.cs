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
    public sealed class RoleAssignmentService : IRoleAssignmentService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleAssignmentService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task AssignRoleAsync(ApplicationUser user, string role, CancellationToken ct = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count > 0)
                throw new InvalidOperationException("User already has a role. Multiple roles are not allowed.");

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
                throw new ApplicationException($"Role assignment failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
