using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateClassCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.Id == request.BranchId &&
                    b.TenantId == tenantId,
                    cancellationToken);

            if (branch is null)
                throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");


            var entity = new Class
            {
                Name = request.Name.Trim(),
                BranchId = request.BranchId,
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
