using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateTenantCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var normalizedSubdomain = request.Subdomain.Trim().ToLowerInvariant();

            var exists = await _context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Subdomain.ToLower() == normalizedSubdomain, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Bu subdomain zaten kullanımda.");

            var now = DateTime.UtcNow;

            var start = request.SubscriptionStartDate ?? now;
            var end = request.SubscriptionEndDate ?? now.AddYears(1);

            var plan = string.IsNullOrWhiteSpace(request.SubscriptionPlan)
                ? "Basic"
                : request.SubscriptionPlan!.Trim();

            var tenant = new Tenant
            {
                Name = request.Name.Trim(),
                Subdomain = normalizedSubdomain,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                LogoUrl = request.LogoUrl,
                PrimaryColor = string.IsNullOrWhiteSpace(request.PrimaryColor) ? "#3B82F6" : request.PrimaryColor!,
                SecondaryColor = string.IsNullOrWhiteSpace(request.SecondaryColor) ? "#1E40AF" : request.SecondaryColor!,
                SubscriptionPlan = plan,
                SubscriptionStartDate = start,
                SubscriptionEndDate = end,
                IsActive = true,
                CreatedAt = now
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            await SeedDefaultRolePermissionsAsync(tenant.Id, now, cancellationToken);

            return tenant.Id;
        }

        private async Task SeedDefaultRolePermissionsAsync(int tenantId, DateTime nowUtc, CancellationToken ct)
        {
            var list = new List<TenantRolePermission>();

            void Add(string role, string key, PermissionScope scope)
            {
                list.Add(new TenantRolePermission
                {
                    TenantId = tenantId,
                    RoleName = role,
                    PermissionKey = key,
                    Scope = scope,
                    IsActive = true,
                    CreatedAt = nowUtc
                });
            }

            // ---- Admin (tenant full) ----
            var admin = RoleNames.Admin;

            Add(admin, PermissionNames.Announcements.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Announcements.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Announcements.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Announcements.Publish, PermissionScope.Tenant);
            Add(admin, PermissionNames.Announcements.Delete, PermissionScope.Tenant);

            Add(admin, PermissionNames.Attendance.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Attendance.Take, PermissionScope.Tenant);
            Add(admin, PermissionNames.Attendance.Edit, PermissionScope.Tenant);
            Add(admin, PermissionNames.Attendance.ReportsRead, PermissionScope.Tenant);
            Add(admin, PermissionNames.Attendance.Export, PermissionScope.Tenant);

            Add(admin, PermissionNames.Branches.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Branches.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Branches.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Branches.Archive, PermissionScope.Tenant);
            Add(admin, PermissionNames.Branches.Delete, PermissionScope.Tenant);

            Add(admin, PermissionNames.Classes.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.Archive, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.Delete, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.AssignCoach, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.EnrollStudents, PermissionScope.Tenant);
            Add(admin, PermissionNames.Classes.UnenrollStudents, PermissionScope.Tenant);

            Add(admin, PermissionNames.Coaches.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Coaches.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Coaches.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Coaches.Archive, PermissionScope.Tenant);
            Add(admin, PermissionNames.Coaches.Delete, PermissionScope.Tenant);
            Add(admin, PermissionNames.Coaches.AssignClass, PermissionScope.Tenant);

            Add(admin, PermissionNames.Documents.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Documents.Upload, PermissionScope.Tenant);
            Add(admin, PermissionNames.Documents.Delete, PermissionScope.Tenant);
            Add(admin, PermissionNames.Documents.DefinitionsManage, PermissionScope.Tenant);

            Add(admin, PermissionNames.Payments.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.CreatePlan, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.UpdatePlan, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.RecordPayment, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.Adjust, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.ReportsRead, PermissionScope.Tenant);
            Add(admin, PermissionNames.Payments.Export, PermissionScope.Tenant);

            Add(admin, PermissionNames.Students.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.Archive, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.Delete, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.AssignClass, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.ChangeClass, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.AttendanceRead, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.PaymentsRead, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.DocumentsRead, PermissionScope.Tenant);
            Add(admin, PermissionNames.Students.DocumentsManage, PermissionScope.Tenant);

            // Progress (Admin full)
            Add(admin, PermissionNames.ProgressMetrics.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressMetrics.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressMetrics.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressMetrics.Delete, PermissionScope.Tenant);

            Add(admin, PermissionNames.ProgressRecords.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressRecords.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressRecords.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.ProgressRecords.Delete, PermissionScope.Tenant);

            Add(admin, PermissionNames.Users.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Users.Create, PermissionScope.Tenant);
            Add(admin, PermissionNames.Users.Update, PermissionScope.Tenant);
            Add(admin, PermissionNames.Users.Disable, PermissionScope.Tenant);
            Add(admin, PermissionNames.Users.Delete, PermissionScope.Tenant);

            Add(admin, PermissionNames.Reports.Read, PermissionScope.Tenant);
            Add(admin, PermissionNames.Reports.Export, PermissionScope.Tenant);
            Add(admin, PermissionNames.Settings.TenantSettingsManage, PermissionScope.Tenant);
            Add(admin, PermissionNames.Settings.IntegrationsManage, PermissionScope.Tenant);

            Add(admin, PermissionNames.Permissions.Explain, PermissionScope.Tenant);

            // ---- Coach (default: ownClasses / branch) ----
            var coach = RoleNames.Coach;

            Add(coach, PermissionNames.Classes.Read, PermissionScope.OwnClasses);

            Add(coach, PermissionNames.Attendance.Read, PermissionScope.OwnClasses);
            Add(coach, PermissionNames.Attendance.Take, PermissionScope.OwnClasses);

            Add(coach, PermissionNames.Students.Read, PermissionScope.OwnClasses);
            Add(coach, PermissionNames.Students.AttendanceRead, PermissionScope.OwnClasses);

            Add(coach, PermissionNames.Documents.Read, PermissionScope.OwnClasses);
            Add(coach, PermissionNames.Documents.Upload, PermissionScope.OwnClasses);

            Add(coach, PermissionNames.Announcements.Read, PermissionScope.OwnClasses);

            // Progress (Coach)
            Add(coach, PermissionNames.ProgressMetrics.Read, PermissionScope.Branch);
            Add(coach, PermissionNames.ProgressRecords.Read, PermissionScope.OwnClasses);
            Add(coach, PermissionNames.ProgressRecords.Create, PermissionScope.OwnClasses);
            Add(coach, PermissionNames.ProgressRecords.Update, PermissionScope.OwnClasses);

            // ---- Student (self) ----
            var student = RoleNames.Student;

            Add(student, PermissionNames.Announcements.ReadPublic, PermissionScope.Self);
            Add(student, PermissionNames.Announcements.Read, PermissionScope.Self);
            Add(student, PermissionNames.Profiles.ReadSelf, PermissionScope.Self);
            Add(student, PermissionNames.Profiles.UpdateSelf, PermissionScope.Self);
            Add(student, PermissionNames.Profiles.ChangePasswordSelf, PermissionScope.Self);

            Add(student, PermissionNames.Students.Read, PermissionScope.Self);
            Add(student, PermissionNames.Students.AttendanceRead, PermissionScope.Self);
            Add(student, PermissionNames.Attendance.Read, PermissionScope.Self);
            Add(student, PermissionNames.Students.PaymentsRead, PermissionScope.Self);
            Add(student, PermissionNames.Students.DocumentsRead, PermissionScope.Self);

            Add(student, PermissionNames.ProgressRecords.Read, PermissionScope.Self);

            // ---- Finance (tenant financial) ----
            var finance = RoleNames.Finance;

            Add(finance, PermissionNames.Payments.Read, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.CreatePlan, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.UpdatePlan, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.RecordPayment, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.Adjust, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.ReportsRead, PermissionScope.Tenant);
            Add(finance, PermissionNames.Payments.Export, PermissionScope.Tenant);

            Add(finance, PermissionNames.Students.Read, PermissionScope.Tenant);
            Add(finance, PermissionNames.Students.PaymentsRead, PermissionScope.Tenant);

            Add(finance, PermissionNames.Reports.Read, PermissionScope.Tenant);

            _context.TenantRolePermissions.AddRange(list);
            await _context.SaveChangesAsync(ct);
        }
    }
}
