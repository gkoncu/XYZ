using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace XYZ.Infrastructure.MultiTenancy
{
    public interface ITenantService
    {
        int GetCurrentTenantId();
    }

    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentTenantId()
        {
            // TODO: Get from subdomain/claim
            return 1;
        }
    }
}
