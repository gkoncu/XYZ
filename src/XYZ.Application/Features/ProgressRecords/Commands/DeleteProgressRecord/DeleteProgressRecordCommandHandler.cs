using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord
{
    public class DeleteProgressRecordCommandHandler
    : IRequestHandler<DeleteProgressRecordCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public DeleteProgressRecordCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(DeleteProgressRecordCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Gelişim kaydı silme yetkiniz yok.");

            var entity = await _dataScope.ProgressRecords()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("ProgressRecord", request.Id);

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }
    }
}
