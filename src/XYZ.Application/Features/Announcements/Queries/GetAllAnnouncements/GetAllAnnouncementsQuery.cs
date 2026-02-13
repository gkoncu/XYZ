using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements
{
    public sealed class GetAllAnnouncementsQuery
        : IRequest<PaginationResult<AnnouncementListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Announcements.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public string? SearchTerm { get; set; }

        public int? ClassId { get; set; }

        public AnnouncementType? Type { get; set; }

        public bool OnlyCurrent { get; set; } = true;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
