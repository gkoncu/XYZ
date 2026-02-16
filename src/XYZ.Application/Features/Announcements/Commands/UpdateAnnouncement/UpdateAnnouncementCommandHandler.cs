using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement
{
    public sealed class UpdateAnnouncementCommandHandler
        : IRequestHandler<UpdateAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly IPermissionService _permissions;

        public UpdateAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            IPermissionService permissions)
        {
            _context = context;
            _dataScope = dataScope;
            _permissions = permissions;
        }

        public async Task<int> Handle(UpdateAnnouncementCommand request, CancellationToken ct)
        {
            if (request.Type == AnnouncementType.System)
                throw new InvalidOperationException("Sistem duyurusu güncellemesi ayrı akışla yönetilmelidir.");

            var announcement = await _dataScope.Announcements()
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

            if (announcement is null)
                throw new NotFoundException("Announcement", request.Id);

            var scope = await _permissions.GetScopeAsync(PermissionNames.Announcements.Update, ct);
            if (!scope.HasValue)
                throw new UnauthorizedAccessException("Duyuru güncelleme yetkiniz yok.");

            if (!announcement.ClassId.HasValue && scope.Value < PermissionScope.Tenant)
                throw new UnauthorizedAccessException("Genel duyuru güncellemek için tenant düzeyi yetki gerekir.");

            if (!request.ClassId.HasValue && scope.Value < PermissionScope.Tenant)
                throw new UnauthorizedAccessException("Genel duyuruya çevirmek için tenant düzeyi yetki gerekir.");

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
