using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Queries.GetAllClasses
{
    public class ClassListItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string BranchName { get; set; } = string.Empty;

        public int StudentsCount { get; set; }

        public int CoachesCount { get; set; }

        public bool IsActive { get; set; }
    }
}
