using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public sealed class CreateCoachCommand : IRequest<int>
    {
        public string UserId { get; set; } = string.Empty;

        public int BranchId { get; set; }

        public string? IdentityNumber { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
