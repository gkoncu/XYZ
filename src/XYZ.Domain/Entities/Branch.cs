using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Branch : TenantScopedEntity
    {
        public string Name { get; set; } = string.Empty;

        public Tenant Tenant { get; set; } = null!;
        public ICollection<Class> Classes { get; set; } = new List<Class>();
        public ICollection<Coach> Coaches { get; set; } = new List<Coach>();
    }
}
