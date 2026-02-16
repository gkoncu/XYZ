using MediatR;
using System;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants
{
    public sealed class CreateSystemAnnouncementForAllTenantsCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Announcements.Create;
        public PermissionScope? MinimumScope => PermissionScope.AllTenants;

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public AnnouncementType Type { get; set; } = AnnouncementType.System;
    }
}
