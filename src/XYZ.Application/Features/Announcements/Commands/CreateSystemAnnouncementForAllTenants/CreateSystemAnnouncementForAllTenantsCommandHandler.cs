using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants
{
    [Authorize(Roles = "SuperAdmin")]
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
            var tenantIds = await _db.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            if (tenantIds.Count == 0)
                return 0;

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
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
            return tenantIds.Count;
        }
    }
}
