using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;
using XYZ.Infrastructure.Data;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.ExternalServices
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;

        public TenantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Tenant?> GetTenantByIdAsync(int id)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
        {
            return await _context.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain);
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
        {
            return !await _context.Tenants
                .AnyAsync(t => t.Subdomain == subdomain);
        }
    }
}
