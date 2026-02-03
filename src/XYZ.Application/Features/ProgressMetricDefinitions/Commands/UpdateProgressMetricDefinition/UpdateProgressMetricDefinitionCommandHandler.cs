using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition
{
    public class UpdateProgressMetricDefinitionCommandHandler : IRequestHandler<UpdateProgressMetricDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateProgressMetricDefinitionCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateProgressMetricDefinitionCommand request, CancellationToken ct)
        {
            var entity = await _context.ProgressMetricDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                throw new NotFoundException("ProgressMetricDefinition", request.Id);

            await _dataScope.EnsureBranchAccessAsync(entity.BranchId, ct);

            if (request.BranchId != entity.BranchId)
                throw new InvalidOperationException("Metrik şubesi değiştirilemez.");

            var name = request.Name.Trim();

            var exists = await _context.ProgressMetricDefinitions
                .AnyAsync(x => x.BranchId == entity.BranchId && x.Id != entity.Id && x.Name == name, ct);

            if (exists)
                throw new InvalidOperationException("Bu şubede aynı isimde başka bir metrik zaten var.");

            entity.Name = name;
            entity.DataType = request.DataType;
            entity.Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim();
            entity.IsRequired = request.IsRequired;
            entity.SortOrder = request.SortOrder;
            entity.MinValue = request.MinValue;
            entity.MaxValue = request.MaxValue;
            entity.IsActive = request.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
