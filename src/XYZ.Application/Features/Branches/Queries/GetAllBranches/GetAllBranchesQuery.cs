using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Branches.Queries.GetAllBranches
{
    public class GetAllBranchesQuery : IRequest<PaginationResult<BranchListItemDto>>
    {
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }
}
