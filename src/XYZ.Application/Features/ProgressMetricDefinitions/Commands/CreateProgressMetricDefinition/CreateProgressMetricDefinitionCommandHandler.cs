using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition
{
    public class CreateProgressMetricDefinitionCommandHandler : IRequestHandler<CreateProgressMetricDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public CreateProgressMetricDefinitionCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(CreateProgressMetricDefinitionCommand request, CancellationToken ct)
        {
            await _dataScope.EnsureBranchAccessAsync(request.BranchId, ct);

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId, ct);
            if (branch is null)
                throw new KeyNotFoundException("Şube bulunamadı.");

            var name = request.Name.Trim();

            var exists = await _context.ProgressMetricDefinitions
                .AnyAsync(x => x.BranchId == request.BranchId && x.Name == name, ct);

            if (exists)
                throw new InvalidOperationException("Bu şubede aynı isimde bir metrik zaten var.");

            var entity = new ProgressMetricDefinition
            {
                TenantId = branch.TenantId,
                BranchId = request.BranchId,
                Name = name,
                DataType = request.DataType,
                Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
                IsRequired = request.IsRequired,
                SortOrder = request.SortOrder,
                MinValue = request.MinValue,
                MaxValue = request.MaxValue,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProgressMetricDefinitions.Add(entity);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
