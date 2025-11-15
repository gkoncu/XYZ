using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Commands.UpdateTenant
{
    public class UpdateTenantCommand : IRequest<int>
    {
        public int TenantId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;

        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? LogoUrl { get; set; }

        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }

        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
