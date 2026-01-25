namespace XYZ.Application.Features.Profile.Queries.GetMyProfile;

public sealed class MyProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public int? TenantId { get; set; }
    public string? TenantName { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public int? BranchId { get; set; }
    public string? BranchName { get; set; }

    public int? ClassId { get; set; }
    public string? ClassName { get; set; }

    public int? ClassesCount { get; set; }

    public int? AdminId { get; set; }
    public bool? CanManageUsers { get; set; }
    public bool? CanManageFinance { get; set; }
    public bool? CanManageSettings { get; set; }
}
