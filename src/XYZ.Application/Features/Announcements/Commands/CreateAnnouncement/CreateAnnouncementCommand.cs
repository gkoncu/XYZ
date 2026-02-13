using MediatR;
using System;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateAnnouncement
{
    public sealed class CreateAnnouncementCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Announcements.Create;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int? ClassId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime? PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public AnnouncementType Type { get; set; } = AnnouncementType.General;
    }
}
