using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Queries.GetBranchById
{
    public class GetBranchByIdQuery : IRequest<BranchDetailDto>
    {
        public int BranchId { get; set; }
    }
}
