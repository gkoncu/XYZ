using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Announcements.Queries.GetAnnouncementById
{
    public class GetAnnouncementByIdQueryHandler
        : IRequestHandler<GetAnnouncementByIdQuery, AnnouncementDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetAnnouncementByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<AnnouncementDetailDto> Handle(
            GetAnnouncementByIdQuery request,
            CancellationToken ct)
        {
            var entity = await _dataScope.Announcements()
                .Include(a => a.Class)
                .Include(a => a.Tenant)
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

            if (entity is null)
                throw new NotFoundException("Announcement", request.Id);

            return new AnnouncementDetailDto
            {
                Id = entity.Id,
                TenantId = entity.TenantId,
                TenantName = entity.Tenant?.Name,
                ClassId = entity.ClassId,
                ClassName = entity.Class?.Name,
                Title = entity.Title,
                Content = entity.Content,
                PublishDate = entity.PublishDate,
                ExpiryDate = entity.ExpiryDate,
                Type = entity.Type,
                IsActive = entity.IsActive
            };
        }
    }
}
