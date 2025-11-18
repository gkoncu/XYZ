using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Announcements.Commands.CreateAnnouncement
{
    public class CreateAnnouncementCommandHandler
        : IRequestHandler<CreateAnnouncementCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreateAnnouncementCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CreateAnnouncementCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Duyuru oluşturma yetkiniz yok.");

            var tenantId = _current.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

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
                CreatedAt = now
            };

            await _context.Announcements.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }
    }
}
