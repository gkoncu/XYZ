using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Branches.Queries.GetBranchById
{
    public class GetBranchByIdQueryHandler
        : IRequestHandler<GetBranchByIdQuery, BranchDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetBranchByIdQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<BranchDetailDto> Handle(
            GetBranchByIdQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var branch = await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == request.BranchId && b.TenantId == tenantId)
                .Select(b => new BranchDetailDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    TenantId = b.TenantId,
                    TenantName = b.Tenant.Name,
                    ClassCount = b.Classes.Count(c => c.IsActive),
                    CreatedAt = b.CreatedAt,
                    IsActive = b.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (branch is null)
                throw new NotFoundException("Branch", request.BranchId);

            return branch;
        }
    }
}
