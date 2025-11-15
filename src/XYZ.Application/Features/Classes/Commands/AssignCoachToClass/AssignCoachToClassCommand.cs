using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.AssignCoachToClass
{
    public class AssignCoachToClassCommand : IRequest<int>
    {
        public int ClassId { get; set; }
        public int CoachId { get; set; }
    }
}
