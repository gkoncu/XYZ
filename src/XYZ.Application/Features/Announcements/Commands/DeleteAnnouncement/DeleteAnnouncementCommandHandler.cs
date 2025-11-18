using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement
{
    public class DeleteAnnouncementCommandHandler
        : IRequestHandler<DeleteAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public DeleteAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(DeleteAnnouncementCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Duyuru silme yetkiniz yok.");

            var announcement = await _dataScope.Announcements()
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

            if (announcement is null)
                throw new NotFoundException("Announcement", request.Id);

            announcement.IsActive = false;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return announcement.Id;
        }
    }
}
