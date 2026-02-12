using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class ClassEnrollment : TenantScopedEntity
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public Student Student { get; set; } = null!;
        public Class Class { get; set; } = null!;
    }
}
