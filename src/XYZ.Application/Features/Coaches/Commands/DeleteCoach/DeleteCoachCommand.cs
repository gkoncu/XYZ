using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach
{
    public class DeleteCoachCommand : IRequest<int>
    {
        public int CoachId { get; set; }
    }
}
