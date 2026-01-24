using MediatR;
using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants
{
    public sealed class CreateSystemAnnouncementForAllTenantsCommand : IRequest<int>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public AnnouncementType Type { get; set; } = AnnouncementType.System;
    }
}
