using XYZ.Application.Features.Tenants.Queries.GetTenantsById;

namespace XYZ.Web.Models.Tenants
{
    public sealed class TenantDetailsViewModel
    {
        public TenantDetailDto Tenant { get; init; } = default!;
    }
}
