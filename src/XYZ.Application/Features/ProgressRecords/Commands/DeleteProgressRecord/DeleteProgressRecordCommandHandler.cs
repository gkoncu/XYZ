using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord
{
    public class DeleteProgressRecordCommandHandler : IRequestHandler<DeleteProgressRecordCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public DeleteProgressRecordCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(DeleteProgressRecordCommand request, CancellationToken ct)
        {
            var record = await _dataScope.ProgressRecords()
                .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

            if (record is null)
                throw new KeyNotFoundException("Gelişim kaydı bulunamadı.");

            record.IsActive = false;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return record.Id;
        }
    }
}
