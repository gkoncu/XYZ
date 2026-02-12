using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;

namespace XYZ.Domain.Entities
{
    public class Coach : TenantScopedEntity
    {
        public string UserId { get; set; } = null!;
        public string? IdentityNumber { get; set; }
        public string? LicenseNumber { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
