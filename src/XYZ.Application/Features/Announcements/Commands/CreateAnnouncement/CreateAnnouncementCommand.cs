using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateAnnouncement
{
    public class CreateAnnouncementCommand : IRequest<int>
    {
        public int? ClassId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public AnnouncementType Type { get; set; } = AnnouncementType.General;
    }
}
