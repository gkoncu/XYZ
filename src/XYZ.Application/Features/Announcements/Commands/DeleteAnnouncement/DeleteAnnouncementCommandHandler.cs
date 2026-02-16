using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement
{
    public sealed class DeleteAnnouncementCommandHandler
        : IRequestHandler<DeleteAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly IPermissionService _permissions;

        public DeleteAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            IPermissionService permissions)
        {
            _context = context;
            _dataScope = dataScope;
            _permissions = permissions;
        }

        public async Task<int> Handle(DeleteAnnouncementCommand request, CancellationToken ct)
        {
            var announcement = await _dataScope.Announcements()
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

            if (announcement is null)
                throw new NotFoundException("Announcement", request.Id);

            var scope = await _permissions.GetScopeAsync(PermissionNames.Announcements.Delete, ct);
            if (!scope.HasValue)
                throw new UnauthorizedAccessException("Duyuru silme yetkiniz yok.");

            if (!announcement.ClassId.HasValue && scope.Value < PermissionScope.Tenant)
                throw new UnauthorizedAccessException("Genel duyuru silmek için tenant düzeyi yetki gerekir.");

            announcement.IsActive = false;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return announcement.Id;
        }
    }
}
