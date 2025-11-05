using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Specifications
{
    public class StudentWithDetailsSpecification : BaseSpecification<Student>
    {
        public StudentWithDetailsSpecification(int studentId)
            : base(s => s.Id == studentId)
        {
            AddInclude(s => s.User);
            AddInclude(s => s.Class);
            AddInclude($"{nameof(Class.Coaches)}.{nameof(Coach.User)}");
        }
    }

    public class StudentSearchSpecification : BaseSpecification<Student>
    {
        public StudentSearchSpecification(string searchTerm, int? classId, string branch)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
            }

            if (classId.HasValue)
            {
                AddCriteria(s => s.ClassId == classId.Value);
            }

            if (!string.IsNullOrEmpty(branch))
            {
                AddCriteria(s => s.Branch == branch);
            }
        }
    }
}
