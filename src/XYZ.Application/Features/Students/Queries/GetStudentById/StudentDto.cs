using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Queries.GetStudentById
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Address { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
        public string? Parent1FullName { get; set; }
        public string? Parent2FullName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent2Email { get; set; }
        public string? Notes { get; set; }
        public string? MedicalInformation { get; set; }
    }
}
