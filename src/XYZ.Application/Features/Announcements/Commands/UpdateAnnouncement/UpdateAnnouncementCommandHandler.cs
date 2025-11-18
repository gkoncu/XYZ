using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement
{
    public class UpdateAnnouncementCommandHandler
        : IRequestHandler<UpdateAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public UpdateAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(UpdateAnnouncementCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Duyuru güncelleme yetkiniz yok.");

            var announcement = await _dataScope.Announcements()
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

            if (announcement is null)
                throw new NotFoundException("Announcement", request.Id);

            if (request.ClassId.HasValue)
            {
                var cls = await _dataScope.Classes()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, ct);

                if (cls is null)
                    throw new NotFoundException("Class", request.ClassId.Value);

                announcement.ClassId = request.ClassId;
            }
            else
            {
                announcement.ClassId = null;
            }

            announcement.Title = request.Title.Trim();
            announcement.Content = request.Content.Trim();
            announcement.PublishDate = request.PublishDate;
            announcement.ExpiryDate = request.ExpiryDate;
            announcement.Type = request.Type;

            if (request.IsActive.HasValue)
                announcement.IsActive = request.IsActive.Value;

            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return announcement.Id;
        }
    }
}
