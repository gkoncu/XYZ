using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class GetAllClassesQuery : IRequest<PaginationResult<ClassListItemDto>>
    {
        public string? SearchTerm { get; set; }
        public int? BranchId { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "Name";
        public string? SortDir { get; set; } = "asc";
    }
}
