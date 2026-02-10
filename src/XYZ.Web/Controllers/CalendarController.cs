using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly IApiClient _api;

        public CalendarController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events(DateTime start, DateTime end, CancellationToken ct = default)
        {
            var from = DateOnly.FromDateTime(start.Date);
            var to = DateOnly.FromDateTime(end.Date.AddDays(-1));

            var announcements = await GetCurrentAnnouncementsAsync(ct);

            var sessions = await GetSessionsByRoleAsync(from, to, ct);

            var events = new List<object>();

            foreach (var a in announcements)
            {
                var aStart = a.PublishDate.Date;
                var aEndExclusive = (a.ExpiryDate?.Date ?? a.PublishDate.Date).AddDays(1);

                if (aEndExclusive < start.Date || aStart > end.Date) continue;

                events.Add(new
                {
                    id = $"announcement:{a.Id}",
                    title = a.Title,
                    start = aStart.ToString("yyyy-MM-dd"),
                    end = aEndExclusive.ToString("yyyy-MM-dd"),
                    allDay = true,
                    url = Url.Action("Details", "Announcements", new { id = a.Id })
                });
            }

            foreach (var s in sessions)
            {
                var dt = s.Date.ToDateTime(TimeOnly.MinValue);
                var startDt = dt.Add(s.StartTime.ToTimeSpan());
                var endDt = dt.Add(s.EndTime.ToTimeSpan());

                events.Add(new
                {
                    id = $"session:{s.Id}",
                    title = $"{s.ClassName} · {s.Title}",
                    start = startDt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = endDt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    allDay = false,
                    url = Url.Action("Details", "ClassSessions", new { id = s.Id })
                });
            }

            return Ok(events);
        }

        private async Task<List<AnnouncementListItemDto>> GetCurrentAnnouncementsAsync(CancellationToken ct)
        {
            var query = new Dictionary<string, string?>
            {
                ["OnlyCurrent"] = "true",
                ["PageNumber"] = "1",
                ["PageSize"] = "200"
            };

            var url = QueryHelpers.AddQueryString("announcements", query);
            var resp = await _api.GetAsync(url, ct);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                return new List<AnnouncementListItemDto>();

            if (!resp.IsSuccessStatusCode)
                return new List<AnnouncementListItemDto>();

            var page = await resp.Content.ReadFromJsonAsync<PaginationResult<AnnouncementListItemDto>>(cancellationToken: ct);
            return page?.Items?.ToList() ?? new List<AnnouncementListItemDto>();
        }

        private async Task<List<ClassSessionListItemDto>> GetSessionsByRoleAsync(DateOnly from, DateOnly to, CancellationToken ct)
        {
            var query = new Dictionary<string, string?>
            {
                ["From"] = from.ToString("yyyy-MM-dd"),
                ["To"] = to.ToString("yyyy-MM-dd"),
                ["PageNumber"] = "1",
                ["PageSize"] = "500",
                ["OnlyActive"] = "true",
                ["SortBy"] = "Date",
                ["SortDir"] = "asc"
            };

            var endpoint = User.IsInRole("Student")
                ? "classsessions/my"
                : "classsessions";

            var url = QueryHelpers.AddQueryString(endpoint, query);
            var resp = await _api.GetAsync(url, ct);

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                return new List<ClassSessionListItemDto>();

            if (!resp.IsSuccessStatusCode)
                return new List<ClassSessionListItemDto>();

            var page = await resp.Content.ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);
            return page?.Items?.ToList() ?? new List<ClassSessionListItemDto>();
        }
    }
}
