using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Commands.CreateBranch
{
    public class CreateBranchCommand : IRequest<int>
    {
        public string Name { get; set; } = string.Empty;
    }
}
