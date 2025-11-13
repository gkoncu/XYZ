using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.AssignStudentToClass
{
    public class AssignStudentToClassCommand : IRequest<int>
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
    }
}
