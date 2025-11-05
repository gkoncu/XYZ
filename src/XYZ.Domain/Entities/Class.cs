using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Class : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? AgeGroupMin { get; set; }
        public int? AgeGroupMax { get; set; }
        public int MaxCapacity { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public ICollection<Coach> Coaches { get; set; } = new List<Coach>();
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<ClassSchedule> Schedules { get; set; } = new List<ClassSchedule>();
    }

}
