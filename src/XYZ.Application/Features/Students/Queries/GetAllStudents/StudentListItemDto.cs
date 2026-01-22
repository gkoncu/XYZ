using System;

namespace XYZ.Application.Features.Students.Queries.GetAllStudents
{
    public class StudentListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        public int TenantId { get; set; }
        public string? TenantName { get; set; }

        public int? ClassId { get; set; }
        public string? ClassName { get; set; }
        public string? BranchName { get; set; }

        public bool IsActive { get; set; }
    }
}
