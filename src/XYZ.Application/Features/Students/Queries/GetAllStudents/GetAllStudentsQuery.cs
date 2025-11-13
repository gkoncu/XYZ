using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsQuery : IRequest<PaginationResult<StudentListItemDto>>
    {
        public string? SearchTerm { get; set; }
        public int? ClassId { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "FullName";
        public string? SortDir { get; set; } = "asc";
    }
}
