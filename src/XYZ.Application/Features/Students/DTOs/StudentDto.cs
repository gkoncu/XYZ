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

        public string Branch { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent1PhoneNumber { get; set; }

        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2Email { get; set; }
        public string? Parent2PhoneNumber { get; set; }

        public string? MedicalInformation { get; set; }
        public string? Notes { get; set; }

        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public List<string> CoachNames { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateStudentDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public string Branch { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent1PhoneNumber { get; set; }

        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2Email { get; set; }
        public string? Parent2PhoneNumber { get; set; }

        public string? MedicalInformation { get; set; }
        public string? Notes { get; set; }

        public int? ClassId { get; set; }
    }

    public class UpdateStudentDto
    {
        public string Branch { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent1PhoneNumber { get; set; }

        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2Email { get; set; }
        public string? Parent2PhoneNumber { get; set; }

        public string? MedicalInformation { get; set; }
        public string? Notes { get; set; }

        public int? ClassId { get; set; }
    }
}
