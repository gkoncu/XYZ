using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement
{
    public sealed class DeleteAnnouncementCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Announcements.Delete;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int Id { get; set; }
    }
}
