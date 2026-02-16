using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants
{
    public sealed class CreateSystemAnnouncementForAllTenantsCommandHandler
        : IRequestHandler<CreateSystemAnnouncementForAllTenantsCommand, int>
    {
        private readonly IApplicationDbContext _db;

        public CreateSystemAnnouncementForAllTenantsCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> Handle(CreateSystemAnnouncementForAllTenantsCommand request, CancellationToken cancellationToken)
        {
            if (request.Type != AnnouncementType.System)
                throw new InvalidOperationException("Broadcast sadece System tipinde duyuru gönderebilir.");

            var tenantIds = await _db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            if (tenantIds.Count == 0)
                return 0;

            var now = DateTime.UtcNow;

            foreach (var tenantId in tenantIds)
            {
                _db.Announcements.Add(new Announcement
                {
                    TenantId = tenantId,
                    ClassId = null,
                    Title = request.Title.Trim(),
                    Content = request.Content.Trim(),
                    PublishDate = request.PublishDate,
                    ExpiryDate = request.ExpiryDate,
                    Type = AnnouncementType.System,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = null
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
            return tenantIds.Count;
        }
    }
}
