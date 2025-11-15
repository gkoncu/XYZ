using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Tenants.Queries.GetTenantsById
{
    public class GetTenantByIdQueryHandler
        : IRequestHandler<GetTenantByIdQuery, TenantDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetTenantByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TenantDetailDto> Handle(
            GetTenantByIdQuery request,
            CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .Where(t => t.Id == request.TenantId)
                .Select(t => new TenantDetailDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Subdomain = t.Subdomain,
                    Address = t.Address,
                    Phone = t.Phone,
                    Email = t.Email,
                    LogoUrl = t.LogoUrl,
                    PrimaryColor = t.PrimaryColor,
                    SecondaryColor = t.SecondaryColor,
                    SubscriptionPlan = t.SubscriptionPlan,
                    SubscriptionStartDate = t.SubscriptionStartDate,
                    SubscriptionEndDate = t.SubscriptionEndDate,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ActiveBranchCount = t.Branches.Count(b => b.IsActive),
                    ActiveClassCount = t.Classes.Count(c => c.IsActive),
                    ActiveStudentCount = t.Students.Count(s => s.IsActive),
                    ActiveCoachCount = t.Coaches.Count(c => c.IsActive)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (tenant is null)
                throw new NotFoundException("Tenant", request.TenantId);

            return tenant;
        }
    }
}
