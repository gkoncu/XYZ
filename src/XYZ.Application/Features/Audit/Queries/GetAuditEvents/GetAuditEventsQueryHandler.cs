using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEvents
{
    public sealed class GetAuditEventsQueryHandler
        : IRequestHandler<GetAuditEventsQuery, PaginationResult<AuditEventListItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditEventsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResult<AuditEventListItemDto>> Handle(GetAuditEventsQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
            if (pageSize > 200) pageSize = 200;

            var q = _context.AuditEvents.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.EntityName))
                q = q.Where(x => x.EntityName == request.EntityName);

            if (!string.IsNullOrWhiteSpace(request.EntityKey))
                q = q.Where(x => x.EntityKey == request.EntityKey);

            if (request.Action.HasValue)
                q = q.Where(x => x.Action == request.Action.Value);

            if (!string.IsNullOrWhiteSpace(request.ActorUserId))
                q = q.Where(x => x.ActorUserId == request.ActorUserId);

            if (request.FromUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc >= request.FromUtc.Value);

            if (request.ToUtc.HasValue)
                q = q.Where(x => x.OccurredAtUtc <= request.ToUtc.Value);

            var totalCount = await q.CountAsync(cancellationToken);

            var items = await q
                .OrderByDescending(x => x.OccurredAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditEventListItemDto
                {
                    Id = x.Id,
                    EntityName = x.EntityName,
                    EntityKey = x.EntityKey,
                    Action = x.Action,
                    ActorUserId = x.ActorUserId,
                    OccurredAtUtc = x.OccurredAtUtc
                })
                .ToListAsync(cancellationToken);

            return new PaginationResult<AuditEventListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
