using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int TenantId { get; set; }
        public UserRole Role { get; set; }
        public string Branch { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public BloodType BloodType { get; set; }
        public DateTime BirthDate { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public Tenant Tenant { get; set; } = null!;
        public Student? StudentProfile { get; set; }
        public Coach? CoachProfile { get; set; }
        public Admin? AdminProfile { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
