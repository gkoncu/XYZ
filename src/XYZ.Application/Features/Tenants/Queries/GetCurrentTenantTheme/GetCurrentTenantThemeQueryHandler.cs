using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme
{
    public class GetCurrentTenantThemeQueryHandler
        : IRequestHandler<GetCurrentTenantThemeQuery, TenantThemeDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetCurrentTenantThemeQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        public async Task<TenantThemeDto> Handle(
            GetCurrentTenantThemeQuery request,
            CancellationToken cancellationToken)
        {
            var defaultTheme = new TenantThemeDto();

            var tenantId = _current.TenantId;
            if (tenantId is null)
            {
                return defaultTheme;
            }

            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId.Value, cancellationToken);

            if (tenant == null)
            {
                return defaultTheme;
            }

            return new TenantThemeDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                PrimaryColor = tenant.PrimaryColor,
                SecondaryColor = tenant.SecondaryColor,
                LogoUrl = tenant.LogoUrl
            };
        }
    }
}
