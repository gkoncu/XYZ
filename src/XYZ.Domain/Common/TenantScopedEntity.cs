using System;

namespace XYZ.Domain.Common
{
    /// <summary>
    /// Tenant'a ait olan tüm entity'lerin ortak tabanı.
    /// 
    /// - TenantId zorunludur.
    /// - EF Core Global Query Filter ile otomatik tenant izolasyonu hedeflenir.
    /// </summary>
    public abstract class TenantScopedEntity : BaseEntity
    {
        public int TenantId { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
    }
}
