using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public sealed class CreateStudentCommand : IRequest<Guid>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public DateTime BirthDate { get; init; }
        public string Gender { get; init; } = string.Empty;
        public string IdentityNumber { get; init; } = string.Empty;
        public string BloodType { get; init; } = "Unknown";

        public string? Parent1FirstName { get; init; }
        public string? Parent1LastName { get; init; }
        public string? Parent1Email { get; init; }
        public string? Parent1PhoneNumber { get; init; }

        public string? Parent2FirstName { get; init; }
        public string? Parent2LastName { get; init; }
        public string? Parent2Email { get; init; }
        public string? Parent2PhoneNumber { get; init; }

        public string? Address { get; init; }
        public string? MedicalInformation { get; init; }
        public string? Notes { get; init; }

        public int? ClassId { get; init; }
    }
}
