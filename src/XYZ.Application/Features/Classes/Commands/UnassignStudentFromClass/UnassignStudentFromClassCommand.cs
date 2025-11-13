using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommand : IRequest<int>
    {
        public int StudentId { get; set; }
    }
}
