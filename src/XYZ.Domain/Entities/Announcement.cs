using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class Announcement : BaseEntity
    {
        public int TenantId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsImportant { get; set; }
        public AnnouncementType Type { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }
}
