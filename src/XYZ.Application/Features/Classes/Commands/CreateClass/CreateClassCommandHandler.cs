using MediatR;
using System;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public CreateClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            var branch = await _dataScope.Branches()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

            if (branch is null)
                throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");

            var entity = new Class
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                AgeGroupMin = request.AgeGroupMin,
                AgeGroupMax = request.AgeGroupMax,
                MaxCapacity = request.MaxCapacity,

                BranchId = request.BranchId,
                TenantId = branch.TenantId,

                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }
}
