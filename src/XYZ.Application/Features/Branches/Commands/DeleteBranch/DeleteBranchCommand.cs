using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Commands.DeleteBranch
{
    public class DeleteBranchCommand : IRequest<int>
    {
        public int BranchId { get; set; }
    }
}
