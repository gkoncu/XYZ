using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.DeleteProgressMetricDefinition
{
    public class DeleteProgressMetricDefinitionCommandHandler : IRequestHandler<DeleteProgressMetricDefinitionCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public DeleteProgressMetricDefinitionCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(DeleteProgressMetricDefinitionCommand request, CancellationToken ct)
        {
            var entity = await _context.ProgressMetricDefinitions.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
            if (entity is null)
                throw new NotFoundException("ProgressMetricDefinition", request.Id);

            await _dataScope.EnsureBranchAccessAsync(entity.BranchId, ct);

            var used = await _context.ProgressRecordValues.AnyAsync(v => v.ProgressMetricDefinitionId == entity.Id && v.IsActive, ct);
            if (used)
                throw new InvalidOperationException("Bu metrik bazı gelişim kayıtlarında kullanıldığı için silinemez. Pasif yapabilirsiniz.");

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
