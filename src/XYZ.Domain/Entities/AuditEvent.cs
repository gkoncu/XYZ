using System;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    /// <summary>
    /// Denetim kaydı (kim, neyi, ne zaman değiştirdi).
    /// Interceptor ile otomatik yazılır; handler'larda audit kodu bulunmaz.
    /// </summary>
    public sealed class AuditEvent : TenantScopedEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityKey { get; set; } = string.Empty;

        public AuditAction Action { get; set; }

        public string? ActorUserId { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Default: minimal (değişen alanlar + new değerler). Hassas alanlar yazılmaz.
        /// </summary>
        public string? ChangesJson { get; set; }
    }
}
