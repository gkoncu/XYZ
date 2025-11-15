using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetTenantsById
{
    public class TenantDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }

        public string PrimaryColor { get; set; } = "#3B82F6";
        public string SecondaryColor { get; set; } = "#1E40AF";

        public string SubscriptionPlan { get; set; } = "Basic";
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int ActiveBranchCount { get; set; }
        public int ActiveClassCount { get; set; }
        public int ActiveStudentCount { get; set; }
        public int ActiveCoachCount { get; set; }
    }
}
