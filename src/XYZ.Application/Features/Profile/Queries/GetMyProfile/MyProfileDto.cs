using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Queries.GetMyProfile;

public sealed class MyProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public int TenantId { get; set; }
    public string? TenantName { get; set; }

    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public DateTime BirthDate { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public int? StudentProfileId { get; set; }
    public int? CoachProfileId { get; set; }

    public string? BranchName { get; set; }
    public string? ClassName { get; set; }
}
