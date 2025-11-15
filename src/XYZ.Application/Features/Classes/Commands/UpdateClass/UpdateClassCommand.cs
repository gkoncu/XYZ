using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommand : IRequest<int>
    {
        public int ClassId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int BranchId { get; set; }
    }
}
