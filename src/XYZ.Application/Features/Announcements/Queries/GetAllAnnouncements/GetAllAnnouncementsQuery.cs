using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements
{
    public class GetAllAnnouncementsQuery
        : IRequest<PaginationResult<AnnouncementListItemDto>>
    {
        public string? SearchTerm { get; set; }

        public int? ClassId { get; set; }

        public AnnouncementType? Type { get; set; }

        public bool OnlyCurrent { get; set; } = true;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
