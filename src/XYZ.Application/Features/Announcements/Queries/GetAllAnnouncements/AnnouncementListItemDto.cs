using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements
{
    public class AnnouncementListItemDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public AnnouncementType Type { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? ClassId { get; set; }

        public string? ClassName { get; set; }

        public bool IsActive { get; set; }
    }
}
