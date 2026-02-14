using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEventById
{
    public sealed class GetAuditEventByIdQueryHandler
        : IRequestHandler<GetAuditEventByIdQuery, AuditEventDetailDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditEventByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditEventDetailDto?> Handle(GetAuditEventByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.AuditEvents
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Select(x => new AuditEventDetailDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    EntityName = x.EntityName,
                    EntityKey = x.EntityKey,
                    Action = x.Action,
                    ActorUserId = x.ActorUserId,
                    OccurredAtUtc = x.OccurredAtUtc,
                    ChangesJson = x.ChangesJson
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
