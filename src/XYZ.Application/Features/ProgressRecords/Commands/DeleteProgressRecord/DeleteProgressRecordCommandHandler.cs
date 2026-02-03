using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord
{
    public class DeleteProgressRecordCommandHandler : IRequestHandler<DeleteProgressRecordCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public DeleteProgressRecordCommandHandler(IApplicationDbContext context, IDataScopeService dataScope, ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<int> Handle(DeleteProgressRecordCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Gelişim kaydı silme yetkiniz yok.");

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
