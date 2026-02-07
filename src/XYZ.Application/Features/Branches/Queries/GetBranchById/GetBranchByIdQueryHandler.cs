using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Branches.Queries.GetBranchById
{
    public class GetBranchByIdQueryHandler
        : IRequestHandler<GetBranchByIdQuery, BranchDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetBranchByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<BranchDetailDto> Handle(
            GetBranchByIdQuery request,
            CancellationToken cancellationToken)
        {
            var branch = await _dataScope.Branches()
                .AsNoTracking()
                .Where(b => b.Id == request.BranchId)
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
