using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEventById
{
    public sealed class AuditEventDetailDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityKey { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string? ActorUserId { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string? ChangesJson { get; set; }
    }
}
