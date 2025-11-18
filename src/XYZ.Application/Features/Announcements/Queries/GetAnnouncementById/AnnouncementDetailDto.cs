using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Queries.GetAnnouncementById
{
    public class AnnouncementDetailDto
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public string? TenantName { get; set; }

        public int? ClassId { get; set; }

        public string? ClassName { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public AnnouncementType Type { get; set; }

        public bool IsActive { get; set; }
    }
}
