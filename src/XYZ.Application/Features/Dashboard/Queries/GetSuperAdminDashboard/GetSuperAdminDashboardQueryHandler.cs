using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard
{
    public class GetSuperAdminDashboardQueryHandler
        : IRequestHandler<GetSuperAdminDashboardQuery, SuperAdminDashboardDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public GetSuperAdminDashboardQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<SuperAdminDashboardDto> Handle(
            GetSuperAdminDashboardQuery request,
            CancellationToken ct)
        {
            if (_current.Role != "SuperAdmin")
                throw new UnauthorizedAccessException("Bu dashboard sadece SuperAdmin içindir.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var totalTenants = await _context.Tenants.CountAsync(ct);
            var activeTenants = await _context.Tenants
                .CountAsync(t => t.IsActive, ct);

            var totalStudents = await _context.Students.CountAsync(ct);
            var totalCoaches = await _context.Coaches.CountAsync(ct);
            var totalClasses = await _context.Classes.CountAsync(ct);

            var totalPayments = await _context.Payments.CountAsync(ct);

            var totalPaidAmount = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount - (p.DiscountAmount ?? 0m), ct);

            var upcomingSessions = await _context.ClassSessions
                .CountAsync(cs =>
                    cs.Date >= today &&
                    cs.Status == SessionStatus.Planned &&
                    cs.IsActive,
                    ct);

            return new SuperAdminDashboardDto
            {
                TotalTenants = totalTenants,
                ActiveTenants = activeTenants,
                TotalStudents = totalStudents,
                TotalCoaches = totalCoaches,
                TotalClasses = totalClasses,
                TotalPayments = totalPayments,
                TotalPaidAmount = totalPaidAmount,
                UpcomingSessions = upcomingSessions
            };
        }
    }
}
