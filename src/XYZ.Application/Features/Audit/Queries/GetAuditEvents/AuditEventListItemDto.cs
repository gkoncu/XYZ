using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEvents
{
    public sealed class AuditEventListItemDto
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityKey { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string? ActorUserId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
    }
}
