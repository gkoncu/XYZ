using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Queries.GetAnnouncementById
{
    public sealed class GetAnnouncementByIdQuery : IRequest<AnnouncementDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Announcements.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
    }
}
