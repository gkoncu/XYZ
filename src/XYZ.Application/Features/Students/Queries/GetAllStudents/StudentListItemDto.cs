using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class StudentListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ClassName { get; set; }
        public string Branch { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
