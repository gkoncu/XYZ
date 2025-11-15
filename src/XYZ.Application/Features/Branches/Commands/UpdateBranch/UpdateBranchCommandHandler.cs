using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Branches.Commands.UpdateBranch
{
    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public UpdateBranchCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var branch = await _context.Branches
                .FirstOrDefaultAsync(
                    b => b.Id == request.BranchId && b.TenantId == tenantId,
                    cancellationToken);

            if (branch is null)
                throw new NotFoundException("Branch", request.BranchId);

            branch.Name = request.Name.Trim();
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return branch.Id;
        }
    }
}
