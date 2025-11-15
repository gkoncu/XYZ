using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateClassCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
        {
            var @class = await _dataScope.Classes()
                .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);

            if (@class is null)
                throw new NotFoundException("Class", request.ClassId);

            if (@class.BranchId != request.BranchId)
            {
                var branchOk = await _context.Branches
                    .AnyAsync(b =>
                        b.Id == request.BranchId &&
                        b.TenantId == @class.TenantId,
                        cancellationToken);

                if (!branchOk)
                    throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");
            }

            @class.Name = request.Name.Trim();
            @class.Description = request.Description;
            @class.AgeGroupMin = request.AgeGroupMin;
            @class.AgeGroupMax = request.AgeGroupMax;
            @class.MaxCapacity = request.MaxCapacity;

            @class.BranchId = request.BranchId;
            @class.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return @class.Id;
        }
    }
}
