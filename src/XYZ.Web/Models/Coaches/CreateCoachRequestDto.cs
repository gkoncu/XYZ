using System;

namespace XYZ.Web.Models.Coaches
{
    public sealed class CreateCoachRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; } = "PreferNotToSay";
        public string BloodType { get; set; } = "Unknown";

        public int BranchId { get; set; }

        public string? IdentityNumber { get; set; }
        public string? LicenseNumber { get; set; }

        public int? TenantId { get; set; }
    }
}
