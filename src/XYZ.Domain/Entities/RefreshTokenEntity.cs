using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Domain.Entities
{
    public sealed class RefreshToken
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = null!;

        public string Hash { get; set; } = null!;

        public int? TenantId { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }
        public DateTimeOffset? RevokedAtUtc { get; set; }
        public Guid? ReplacedByTokenId { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
    }
}

