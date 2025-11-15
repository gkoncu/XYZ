using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Commands.UpdateBranch
{
    public class UpdateBranchCommand : IRequest<int>
    {
        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
