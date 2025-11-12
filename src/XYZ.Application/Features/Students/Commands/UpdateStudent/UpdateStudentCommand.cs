using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommand : IRequest<bool>
    {
        public int StudentId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }

        public string IdentityNumber { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public string? Address { get; set; }

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
    }
}
