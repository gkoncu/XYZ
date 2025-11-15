using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Tenants.Commands.UpdateTenant
{
    public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTenantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

            if (tenant is null)
                throw new NotFoundException("Tenant", request.TenantId);

            var newSubdomain = request.Subdomain.Trim().ToLowerInvariant();

            var exists = await _context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t =>
                    t.Id != tenant.Id &&
                    t.Subdomain.ToLower() == newSubdomain,
                    cancellationToken);

            if (exists)
                throw new InvalidOperationException("Bu subdomain zaten kullanımda.");

            tenant.Name = request.Name.Trim();
            tenant.Subdomain = newSubdomain;
            tenant.Address = request.Address;
            tenant.Phone = request.Phone;
            tenant.Email = request.Email;
            tenant.LogoUrl = request.LogoUrl;

            if (!string.IsNullOrWhiteSpace(request.PrimaryColor))
                tenant.PrimaryColor = request.PrimaryColor!;
            if (!string.IsNullOrWhiteSpace(request.SecondaryColor))
                tenant.SecondaryColor = request.SecondaryColor!;

            if (!string.IsNullOrWhiteSpace(request.SubscriptionPlan))
                tenant.SubscriptionPlan = request.SubscriptionPlan!.Trim();

            if (request.SubscriptionStartDate.HasValue)
                tenant.SubscriptionStartDate = request.SubscriptionStartDate.Value;

            if (request.SubscriptionEndDate.HasValue)
                tenant.SubscriptionEndDate = request.SubscriptionEndDate.Value;

            if (request.IsActive.HasValue)
                tenant.IsActive = request.IsActive.Value;

            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return tenant.Id;
        }
    }
}
