using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public class CreateCoachCommand : IRequest<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string? IdentityNumber { get; set; }
        public string? LicenseNumber { get; set; }

        public int BranchId { get; set; }
    }
}
