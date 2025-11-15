using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Branches.Commands.DeleteBranch
{
    public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public DeleteBranchCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var branch = await _context.Branches
                .Include(b => b.Classes)
                .Include(b => b.Coaches)
                .FirstOrDefaultAsync(
                    b => b.Id == request.BranchId && b.TenantId == tenantId,
                    cancellationToken);

            if (branch is null)
                throw new NotFoundException("Branch", request.BranchId);

            var hasActiveClasses = branch.Classes.Any(c => c.IsActive);
            var hasActiveCoaches = branch.Coaches.Any(c => c.IsActive);
            if (hasActiveClasses || hasActiveCoaches)
                throw new InvalidOperationException("Bu branşa bağlı aktif sınıflar veya antrenörler olduğu için silinemez. Önce sınıfları pasif hale getirin veya başka branşa taşıyın.");

            branch.IsActive = false;
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return branch.Id;
        }
    }
}
