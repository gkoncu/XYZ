using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateTenantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var normalizedSubdomain = request.Subdomain.Trim().ToLowerInvariant();

            var exists = await _context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Subdomain.ToLower() == normalizedSubdomain, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Bu subdomain zaten kullanımda.");

            var now = DateTime.UtcNow;

            var start = request.SubscriptionStartDate ?? now;
            var end = request.SubscriptionEndDate ?? now.AddYears(1);

            var plan = string.IsNullOrWhiteSpace(request.SubscriptionPlan)
                ? "Basic"
                : request.SubscriptionPlan!.Trim();

            var tenant = new Tenant
            {
                Name = request.Name.Trim(),
                Subdomain = normalizedSubdomain,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                LogoUrl = request.LogoUrl,
                PrimaryColor = string.IsNullOrWhiteSpace(request.PrimaryColor) ? "#3B82F6" : request.PrimaryColor!,
                SecondaryColor = string.IsNullOrWhiteSpace(request.SecondaryColor) ? "#1E40AF" : request.SecondaryColor!,
                SubscriptionPlan = plan,
                SubscriptionStartDate = start,
                SubscriptionEndDate = end,
                IsActive = true,
                CreatedAt = now
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            return tenant.Id;
        }
    }
}
