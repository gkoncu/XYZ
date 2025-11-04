using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalInformation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string CoachName { get; set; } = string.Empty;
    }

    public class CreateStudentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalInformation { get; set; }
        public int? ClassId { get; set; }
        public int CoachId { get; set; }
    }

    public class UpdateStudentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalInformation { get; set; }
        public int? ClassId { get; set; }
        public int CoachId { get; set; }
    }
}
