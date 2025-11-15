using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.DeleteClass
{
    public class DeleteClassCommand : IRequest<int>
    {
        public int ClassId { get; set; }
    }
}
