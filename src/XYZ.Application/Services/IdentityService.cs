using Microsoft.AspNetCore.Identity;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services;

public class IdentityService : IIdentityService
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public IdentityService(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return new LoginResponse 
            { 
                Succeeded = false, 
                Error = "Geçersiz email adresi veya þifre" 
            };
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        return new LoginResponse
        {
            Succeeded = result.Succeeded,
            Error = result.Succeeded ? null : "Geçersiz email adresi veya þifre",
            ReturnUrl = request.ReturnUrl
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        if (_currentUserService.UserId == null)
            return null;

        return await _userManager.FindByIdAsync(_currentUserService.UserId);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, role);
    }
}