using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UnassignCoachToClass
{
    public class UnassignCoachFromClassCommand : IRequest<int>
    {
        public int CoachId { get; set; }
        public int? ClassId { get; set; }
    }
}
