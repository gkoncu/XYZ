using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommand : IRequest<int>
    {
        public int StudentId { get; set; }
    }
}
