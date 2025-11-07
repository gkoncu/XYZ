using XYZ.Application.Features.Auth.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<ApplicationUser?> GetCurrentUserAsync();
    Task<bool> IsInRoleAsync(string userId, string role);
}