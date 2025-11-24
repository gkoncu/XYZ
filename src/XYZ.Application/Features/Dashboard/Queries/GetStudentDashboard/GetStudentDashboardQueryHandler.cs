using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard
{
    public class GetStudentDashboardQueryHandler
        : IRequestHandler<GetStudentDashboardQuery, StudentDashboardDto>
    {
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public GetStudentDashboardQueryHandler(
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<StudentDashboardDto> Handle(
            GetStudentDashboardQuery request,
            CancellationToken ct)
        {
            if (_current.Role != "Student" || !_current.StudentId.HasValue)
                throw new UnauthorizedAccessException("Bu dashboard sadece öğrenci için kullanılabilir.");

            var studentId = _current.StudentId.Value;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var attQuery = _dataScope.Attendances()
                .Where(a => a.StudentId == studentId);

            var totalSessions = await attQuery.CountAsync(ct);
            var attendedSessions = await attQuery
                .CountAsync(a => a.Status == AttendanceStatus.Present, ct);
            var missedSessions = await attQuery
                .CountAsync(a => a.Status == AttendanceStatus.Absent, ct);

            var upcomingQuery = attQuery
                .Where(a =>
                    a.ClassSession.Date >= today &&
                    a.ClassSession.Status == SessionStatus.Scheduled);

            var upcomingSessionsCount = await upcomingQuery.CountAsync(ct);

            var nextSession = await upcomingQuery
                .OrderBy(a => a.ClassSession.Date)
                .ThenBy(a => a.ClassSession.StartTime)
                .Select(a => new
                {
                    a.ClassSession.Date,
                    a.ClassSession.StartTime,
                    ClassName = a.Class.Name
                })
                .FirstOrDefaultAsync(ct);

            var paymentQuery = _dataScope.Payments()
                .Where(p =>
                    p.StudentId == studentId &&
                    (p.Status == PaymentStatus.Pending ||
                     p.Status == PaymentStatus.Overdue));

            var outstandingPaymentsCount = await paymentQuery.CountAsync(ct);
            var outstandingPaymentsAmount = await paymentQuery
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            return new StudentDashboardDto
            {
                TotalSessions = totalSessions,
                AttendedSessions = attendedSessions,
                MissedSessions = missedSessions,
                UpcomingSessionsCount = upcomingSessionsCount,
                NextSessionDate = nextSession?.Date,
                NextSessionStartTime = nextSession?.StartTime,
                NextSessionClassName = nextSession?.ClassName,
                OutstandingPaymentsCount = outstandingPaymentsCount,
                OutstandingPaymentsAmount = outstandingPaymentsAmount
            };
        }
    }
}
