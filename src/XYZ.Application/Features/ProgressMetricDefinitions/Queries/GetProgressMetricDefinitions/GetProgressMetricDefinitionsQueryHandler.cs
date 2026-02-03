using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitions
{
    public class GetProgressMetricDefinitionsQueryHandler
        : IRequestHandler<GetProgressMetricDefinitionsQuery, IList<ProgressMetricDefinitionListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetProgressMetricDefinitionsQueryHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<IList<ProgressMetricDefinitionListItemDto>> Handle(GetProgressMetricDefinitionsQuery request, CancellationToken ct)
        {
            await _dataScope.EnsureBranchAccessAsync(request.BranchId, ct);

            var q = _context.ProgressMetricDefinitions
                .Where(x => x.BranchId == request.BranchId)
                .AsQueryable();

            if (!request.IncludeInactive)
                q = q.Where(x => x.IsActive);

            return await q
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new ProgressMetricDefinitionListItemDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    BranchId = x.BranchId,
                    Name = x.Name,
                    DataType = x.DataType,
                    Unit = x.Unit,
                    IsRequired = x.IsRequired,
                    SortOrder = x.SortOrder,
                    MinValue = x.MinValue,
                    MaxValue = x.MaxValue,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }
    }
}