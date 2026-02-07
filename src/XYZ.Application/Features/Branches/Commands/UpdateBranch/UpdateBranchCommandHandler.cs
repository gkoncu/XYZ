using MediatR;
using System;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Branches.Commands.UpdateBranch
{
    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateBranchCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _dataScope.Branches()
                .FirstOrDefaultAsync(b => b.Id == request.BranchId, cancellationToken);

            if (branch is null)
                throw new NotFoundException("Branch", request.BranchId);

            branch.Name = request.Name.Trim();
            branch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return branch.Id;
        }
    }
}
