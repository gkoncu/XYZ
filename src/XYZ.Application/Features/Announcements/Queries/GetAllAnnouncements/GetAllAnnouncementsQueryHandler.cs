using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements
{
    public class GetAllAnnouncementsQueryHandler
        : IRequestHandler<GetAllAnnouncementsQuery, PaginationResult<AnnouncementListItemDto>>
    {
        private readonly IDataScopeService _dataScope;

        public GetAllAnnouncementsQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaginationResult<AnnouncementListItemDto>> Handle(
            GetAllAnnouncementsQuery request,
            CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var query = _dataScope.Announcements()
                .Include(a => a.Class)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(a =>
                    a.Title.Contains(term) ||
                    a.Content.Contains(term));
            }

            if (request.ClassId.HasValue)
            {
                query = query.Where(a => a.ClassId == request.ClassId.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(a => a.Type == request.Type.Value);
            }

            if (request.OnlyCurrent)
            {
                query = query.Where(a =>
                    a.PublishDate <= now &&
                    (a.ExpiryDate == null || a.ExpiryDate >= now));
            }

            var totalCount = await query.CountAsync(ct);

            var page = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var size = request.PageSize <= 0 ? 20 : request.PageSize;

            var items = await query
                .OrderByDescending(a => a.PublishDate)
                .ThenByDescending(a => a.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(a => new AnnouncementListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Type = a.Type,
                    PublishDate = a.PublishDate,
                    ExpiryDate = a.ExpiryDate,
                    ClassId = a.ClassId,
                    ClassName = a.Class != null ? a.Class.Name : null,
                    IsActive = a.IsActive
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return new PaginationResult<AnnouncementListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size
            };
        }
    }
}
