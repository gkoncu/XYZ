namespace XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard
{
    public class AdminCoachDashboardDto
    {
        public int StudentCount { get; set; }
        public int ClassCount { get; set; }
        public int UpcomingSessionsCount { get; set; }
        public int TodaySessionsCount { get; set; }
        public int PendingPaymentsCount { get; set; }
        public decimal PendingPaymentsAmount { get; set; }
        public int OverduePaymentsCount { get; set; }
        public decimal OverduePaymentsAmount { get; set; }
        public int UpcomingDuePaymentsCount { get; set; }
        public int ActiveAnnouncementsCount { get; set; }
        public int IncompleteStudentDocumentsCount { get; set; }
        public int IncompleteCoachDocumentsCount { get; set; }
    }
}
