using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Tenants.Commands.DeleteTenant
{
    public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public DeleteTenantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = await _context.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

            if (tenant is null)
                throw new NotFoundException("Tenant", request.TenantId);

            tenant.IsActive = false;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return tenant.Id;
        }
    }
}
