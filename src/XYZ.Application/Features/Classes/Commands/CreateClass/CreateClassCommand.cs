using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommand : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
        public int BranchId { get; set; }
    }
}
