using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces
{
    public interface ITenantService
    {
        Task<Tenant?> GetTenantByIdAsync(int id);
        Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
        Task<List<Tenant>> GetAllTenantsAsync();
        Task<bool> IsSubdomainAvailableAsync(string subdomain);
    }
}
