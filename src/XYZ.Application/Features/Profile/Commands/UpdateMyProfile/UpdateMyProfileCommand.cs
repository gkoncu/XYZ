using MediatR;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommand : IRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public DateTime BirthDate { get; set; }
}
