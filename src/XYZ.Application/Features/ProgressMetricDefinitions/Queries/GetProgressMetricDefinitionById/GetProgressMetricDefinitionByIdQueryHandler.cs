using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById
{
    public class GetProgressMetricDefinitionByIdQueryHandler
        : IRequestHandler<GetProgressMetricDefinitionByIdQuery, ProgressMetricDefinitionDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetProgressMetricDefinitionByIdQueryHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<ProgressMetricDefinitionDetailDto> Handle(GetProgressMetricDefinitionByIdQuery request, CancellationToken ct)
        {
            var def = await _context.ProgressMetricDefinitions
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (def is null)
                throw new NotFoundException("ProgressMetricDefinition", request.Id);

            await _dataScope.EnsureBranchAccessAsync(def.BranchId, ct);

            return new ProgressMetricDefinitionDetailDto
            {
                Id = def.Id,
                TenantId = def.TenantId,
                BranchId = def.BranchId,
                BranchName = def.Branch?.Name,
                Name = def.Name,
                DataType = def.DataType,
                Unit = def.Unit,
                IsRequired = def.IsRequired,
                SortOrder = def.SortOrder,
                MinValue = def.MinValue,
                MaxValue = def.MaxValue,
                IsActive = def.IsActive
            };
        }
    }
}
