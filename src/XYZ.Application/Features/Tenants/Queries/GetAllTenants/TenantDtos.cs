using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetAllTenants
{
    public class TenantListItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string SubscriptionPlan { get; set; } = string.Empty;
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }

        public int ActiveBranchCount { get; set; }
        public int ActiveClassCount { get; set; }
        public int ActiveStudentCount { get; set; }
        public int ActiveCoachCount { get; set; }
    }
}
