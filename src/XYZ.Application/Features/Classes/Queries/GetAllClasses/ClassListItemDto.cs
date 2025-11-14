using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class ClassCoachItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class ClassStudentItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ClassDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int TenantId { get; set; }

        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int StudentsCount { get; set; }
        public int CoachesCount { get; set; }

        public List<ClassCoachItemDto> Coaches { get; set; } = new();
        public List<ClassStudentItemDto> Students { get; set; } = new();
    }
}
