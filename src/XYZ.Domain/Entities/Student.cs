using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public int TenantId { get; set; }
        public int? ClassId { get; set; }

        public string IdentityNumber { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
        public Class? Class { get; set; }
    }
}
