using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Coaches.Queries.GetAllCoaches
{
    public class CoachListItemDto
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string BranchName { get; set; } = string.Empty;

        public int ClassesCount { get; set; }

        public bool IsActive { get; set; }
    }

}
