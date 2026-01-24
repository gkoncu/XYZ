using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class DocumentDefinition : BaseEntity
    {
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public DocumentTarget Target { get; set; }

        public string Name { get; set; } = null!;
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
    }
}
