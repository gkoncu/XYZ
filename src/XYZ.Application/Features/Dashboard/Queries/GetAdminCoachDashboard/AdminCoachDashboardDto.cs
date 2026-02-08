using System;
using System.Collections.Generic;

namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard
{
    public sealed class AdminCoachDashboardDto
    {
        public int StudentCount { get; set; }
        public int ClassCount { get; set; }
        public int TodaySessionsCount { get; set; }
        public int UpcomingSessionsCount { get; set; }

        public int IncompleteStudentDocumentsCount { get; set; }
        public IList<IncompleteStudentListItemDto> IncompleteStudents { get; set; } = new List<IncompleteStudentListItemDto>();

        public int OverduePaymentsCount { get; set; }
        public decimal OverduePaymentsAmount { get; set; }
        public IList<OverdueStudentListItemDto> OverdueStudents { get; set; } = new List<OverdueStudentListItemDto>();

        public IList<TodaySessionListItemDto> TodaySessions { get; set; } = new List<TodaySessionListItemDto>();

        public IList<RecentAnnouncementListItemDto> RecentAnnouncements { get; set; } = new List<RecentAnnouncementListItemDto>();

        public DateTime? SubscriptionEndDate { get; set; }
        public int? SubscriptionDaysRemaining { get; set; }
    }

    public sealed class TodaySessionListItemDto
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public sealed class IncompleteStudentListItemDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int MissingCount { get; set; }
    }

    public sealed class OverdueStudentListItemDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;

        public int OverdueCount { get; set; }
        public decimal OverdueAmount { get; set; }
        public DateTime? OldestDueDate { get; set; }
    }

    public sealed class RecentAnnouncementListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
    }
}
