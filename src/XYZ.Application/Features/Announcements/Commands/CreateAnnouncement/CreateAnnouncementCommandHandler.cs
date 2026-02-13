using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateAnnouncement
{
    public sealed class CreateAnnouncementCommandHandler
        : IRequestHandler<CreateAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;
        private readonly IPermissionService _permissions;

        public CreateAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser,
            IPermissionService permissions)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
            _permissions = permissions;
        }

        public async Task<int> Handle(CreateAnnouncementCommand request, CancellationToken ct)
        {
            var tenantId = _current.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            if (request.Type == AnnouncementType.System)
                throw new InvalidOperationException("Sistem duyurusu için 'Sistem Duyurusu / Broadcast' akışını kullanın.");

            var scope = await _permissions.GetScopeAsync(PermissionNames.Announcements.Create, ct);
            if (!scope.HasValue)
                throw new UnauthorizedAccessException("Duyuru oluşturma yetkiniz yok.");

            if (!request.ClassId.HasValue && scope.Value < PermissionScope.Tenant)
                throw new UnauthorizedAccessException("Genel duyuru oluşturmak için tenant düzeyi yetki gerekir.");

            if (request.ClassId.HasValue)
            {
                var cls = await _dataScope.Classes()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, ct);

                if (cls is null)
                    throw new NotFoundException("Class", request.ClassId.Value);
            }

            var now = DateTime.UtcNow;
            var publishDate = request.PublishDate ?? now;

            var entity = new Announcement
            {
                TenantId = tenantId,
                ClassId = request.ClassId,
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                PublishDate = publishDate,
                ExpiryDate = request.ExpiryDate,
                Type = request.Type,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = null
            };

            await _context.Announcements.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
