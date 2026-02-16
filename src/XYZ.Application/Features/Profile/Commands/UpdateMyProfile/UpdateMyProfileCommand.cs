using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommand : IRequest, IRequirePermission
{
    public string PermissionKey => PermissionNames.Profiles.UpdateSelf;
    public PermissionScope? MinimumScope => PermissionScope.Self;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public DateTime BirthDate { get; set; }
}
