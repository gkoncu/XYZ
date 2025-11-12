using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class GetAllStudentsQuery : IRequest<List<StudentListItemDto>>
    {
        public string? SearchTerm { get; set; }
        public int? ClassId { get; set; }
    }
}
