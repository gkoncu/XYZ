using System;
using System.Collections.Generic;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard
{
    public class SuperAdminDashboardDto
    {
        public int? ActiveTenantId { get; set; }
        public string ActiveTenantName { get; set; } = string.Empty;

        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }

        public int TotalStudents { get; set; }
        public int TotalCoaches { get; set; }
        public int TotalClasses { get; set; }

        public int UpcomingSessions { get; set; }

        public int ExpiringTenantsIn30Days { get; set; }
        public IList<ExpiringTenantListItemDto> ExpiringTenants { get; set; } = new List<ExpiringTenantListItemDto>();

        public SuperAdminSystemHealthDto SystemHealth { get; set; } = new();
    }

    public sealed class ExpiringTenantListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;

        public DateTime SubscriptionEndDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class SuperAdminSystemHealthDto
    {
        public string Environment { get; set; } = string.Empty;
        public bool EmailEnabled { get; set; }
        public DateTime ServerUtcNow { get; set; }
        public string AppVersion { get; set; } = string.Empty;
    }
}
