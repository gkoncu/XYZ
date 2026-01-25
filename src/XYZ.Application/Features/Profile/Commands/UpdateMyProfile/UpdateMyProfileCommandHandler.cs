using MediatR;
using Microsoft.AspNetCore.Identity;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Profile.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _current;

    public UpdateMyProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService current)
    {
        _userManager = userManager;
        _current = current;
    }

    public async Task Handle(UpdateMyProfileCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

        var user = await _userManager.FindByIdAsync(_current.UserId);
        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.Gender = request.Gender;
        user.BloodType = request.BloodType;
        user.BirthDate = request.BirthDate;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var msg = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Profil güncellenemedi: {msg}");
        }
    }
}
