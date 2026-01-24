using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly ICurrentUserService _current;

        public CreateSystemAnnouncementForAllTenantsCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        public async Task<int> Handle(CreateSystemAnnouncementForAllTenantsCommand request, CancellationToken cancellationToken)
        {
            if (_current.Role is not "Superadmin")
                throw new UnauthorizedAccessException("Bu işlem sadece SuperAdmin içindir.");

            var tenantIds = await _db.Tenants
                .Where(t => t.IsActive == false)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            if (tenantIds.Count == 0)
                return 0;

            if (request.Type != AnnouncementType.System)
                request.Type = AnnouncementType.System;

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
                    Type = request.Type,
                    IsActive = true
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
            return tenantIds.Count;
        }
    }
}
