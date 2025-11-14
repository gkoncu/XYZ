using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Queries.GetAllCoaches
{
    public class CoachClassItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CoachDetailDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }

        public int TenantId { get; set; }

        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public string IdentityNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<CoachClassItemDto> Classes { get; set; } = new();
    }

}
