using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard
{
    public sealed class StudentDashboardDto
    {
        public int UpcomingSessions7DaysCount { get; set; }
        public IList<UpcomingSessionListItemDto> UpcomingSessions { get; set; } = new List<UpcomingSessionListItemDto>();

        public int MissingDocumentsCount { get; set; }
        public IList<MissingDocumentListItemDto> MissingDocuments { get; set; } = new List<MissingDocumentListItemDto>();

        public int OverdueFeesCount { get; set; }
        public decimal OverdueFeesAmount { get; set; }
        public IList<FeeListItemDto> OverdueFees { get; set; } = new List<FeeListItemDto>();

        public int DueSoonFeesCount { get; set; }
        public decimal DueSoonFeesAmount { get; set; }
        public IList<FeeListItemDto> DueSoonFees { get; set; } = new List<FeeListItemDto>();

        public IList<RecentAnnouncementListItemDto> RecentAnnouncements { get; set; } = new List<RecentAnnouncementListItemDto>();

        public AttendanceOverallDto AttendanceOverall { get; set; } = new();

        public IList<AttendanceWeeklyTrendItemDto> AttendanceWeeklyTrend { get; set; } = new List<AttendanceWeeklyTrendItemDto>();
    }

    public sealed class UpcomingSessionListItemDto
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }

        public string? CoachName { get; set; }
    }

    public sealed class MissingDocumentListItemDto
    {
        public int DocumentDefinitionId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class FeeListItemDto
    {
        public int PaymentId { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public sealed class RecentAnnouncementListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
    }

    public sealed class AttendanceOverallDto
    {
        public int PresentLikeCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
        public int UnknownCount { get; set; }

        public int AttendancePercent { get; set; }
    }

    public sealed class AttendanceWeeklyTrendItemDto
    {
        public DateOnly WeekStart { get; set; }
        public string Label { get; set; } = string.Empty;
        public int AttendancePercent { get; set; }
    }
}
