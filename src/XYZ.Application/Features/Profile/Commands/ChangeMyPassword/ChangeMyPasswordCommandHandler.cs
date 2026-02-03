using MediatR;
using Microsoft.AspNetCore.Identity;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Profile.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommandHandler : IRequestHandler<ChangeMyPasswordCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _current;

    public ChangeMyPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService current)
    {
        _userManager = userManager;
        _current = current;
    }

    public async Task Handle(ChangeMyPasswordCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(_current.UserId);
        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
            return;

        if (result.Errors.Any(e => string.Equals(e.Code, "PasswordMismatch", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("INVALID_CURRENT_PASSWORD");

        throw new InvalidOperationException("PASSWORD_POLICY_FAILED");
    }
}
