using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Coach : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public int TenantId { get; set; }
        public string Branch { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
