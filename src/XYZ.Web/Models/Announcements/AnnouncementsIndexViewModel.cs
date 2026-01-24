using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements;
using XYZ.Domain.Enums;

namespace XYZ.Web.Models.Announcements
{
    public sealed class AnnouncementsIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public int? ClassId { get; set; }
        public AnnouncementType? Type { get; set; }
        public bool OnlyCurrent { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }

        public List<AnnouncementListItemDto> Items { get; set; } = new();

        public bool CanWrite { get; set; }

        public List<SelectListItem> TypeOptions { get; set; } = new();
    }
}
