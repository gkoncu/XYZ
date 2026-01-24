using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services;

public class DataScopeService : IDataScopeService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;

    public DataScopeService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _current = currentUser;
    }

    // -------- Scoped Root Queries --------
    public IQueryable<Student> Students() => ApplyStudentScope(_context.Students);
    public IQueryable<Class> Classes() => ApplyClassScope(_context.Classes);
    public IQueryable<Coach> Coaches() => ApplyCoachScope(_context.Coaches);
    public IQueryable<Document> Documents() => ApplyDocumentScope(_context.Documents);
    public IQueryable<Attendance> Attendances() => ApplyAttendanceScope(_context.Attendances);
    public IQueryable<ProgressRecord> ProgressRecords() => ApplyProgressScope(_context.ProgressRecords);
    public IQueryable<Payment> Payments() => ApplyPaymentScope(_context.Payments);
    public IQueryable<Announcement> Announcements() => ApplyAnnouncementScope(_context.Announcements);
    public IQueryable<PaymentPlan> PaymentPlans() => ApplyPaymentPlanScope(_context.PaymentPlans);

    // -------- Composition Helpers --------
    public IQueryable<Student> TenantStudents(int tenantId)
        => Students().Where(s => s.TenantId == tenantId);

    public IQueryable<Student> ClassStudents(int classId)
        => Students().Where(s => s.ClassId == classId);

    public IQueryable<Student> CoachStudents(int coachId)
        => Students().Where(s => s.Class != null && s.Class.Coaches.Any(c => c.Id == coachId));

    public IQueryable<Class> CoachClasses(int coachId)
        => Classes().Where(c => c.Coaches.Any(co => co.Id == coachId));

    public IQueryable<PaymentPlan> StudentPaymentPlans(int studentId)
        => PaymentPlans().Where(pp => pp.StudentId == studentId);

    // -------- Guard / CanAccess --------
    public async Task<bool> CanAccessStudentAsync(int studentId, CancellationToken ct = default)
        => await Students().AnyAsync(s => s.Id == studentId, ct);

    public async Task<bool> CanAccessClassAsync(int classId, CancellationToken ct = default)
        => await Classes().AnyAsync(c => c.Id == classId, ct);

    public async Task<bool> CanAccessDocumentAsync(int documentId, CancellationToken ct = default)
        => await Documents().AnyAsync(d => d.Id == documentId, ct);

    public async Task EnsureStudentAccessAsync(int studentId, CancellationToken ct = default)
    {
        if (!await CanAccessStudentAsync(studentId, ct))
            throw new UnauthorizedAccessException("Bu öğrenciye erişiminiz yok.");
    }

    public async Task EnsureClassAccessAsync(int classId, CancellationToken ct = default)
    {
        if (!await CanAccessClassAsync(classId, ct))
            throw new UnauthorizedAccessException("Bu sınıfa erişiminiz yok.");
    }

    // -------- Inside Scope (Role Based WHERE) --------
    private IQueryable<Student> ApplyStudentScope(IQueryable<Student> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(s => s.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(s => s.TenantId == tenantId.Value
                                   && s.Class != null
                                   && s.Class.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(s => s.Id == studentId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<Class> ApplyClassScope(IQueryable<Class> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(c => c.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(c => c.TenantId == tenantId.Value
                                   && c.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(c => c.Students.Any(s => s.Id == studentId.Value))
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<Coach> ApplyCoachScope(IQueryable<Coach> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(c => c.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(c => c.TenantId == tenantId.Value && c.Id == coachId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<Document> ApplyDocumentScope(IQueryable<Document> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        if (role is null) return q.Where(_ => false);

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(d =>
                        (d.StudentId != null && d.Student != null && d.Student.TenantId == tenantId) ||
                        (d.CoachId != null && d.Coach != null && d.Coach.TenantId == tenantId) ||
                        (d.AdminId != null && d.Admin != null && d.Admin.TenantId == tenantId))
                    : q.Where(_ => false);

            case "Coach":
                if (!tenantId.HasValue || !coachId.HasValue) return q.Where(_ => false);

                return q.Where(d =>
                    (d.CoachId != null && d.CoachId == coachId) ||
                    (d.StudentId != null && d.Student != null && d.Student.CoachId == coachId));

            case "Student":
                if (!tenantId.HasValue || !studentId.HasValue) return q.Where(_ => false);

                return q.Where(d => d.StudentId != null && d.StudentId == studentId);

            default:
                return q.Where(_ => false);
        }
    }


    private IQueryable<Attendance> ApplyAttendanceScope(IQueryable<Attendance> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(a => a.Student.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(a => a.Student.TenantId == tenantId.Value
                                   && a.Student.Class != null
                                   && a.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(a => a.StudentId == studentId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<ProgressRecord> ApplyProgressScope(IQueryable<ProgressRecord> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(p => p.Student.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(p => p.Student.TenantId == tenantId.Value
                                   && p.Student.Class != null
                                   && p.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(p => p.StudentId == studentId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<Payment> ApplyPaymentScope(IQueryable<Payment> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(p => p.Student.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(p => p.Student.TenantId == tenantId.Value
                                   && p.Student.Class != null
                                   && p.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(p => p.StudentId == studentId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }

    private IQueryable<Announcement> ApplyAnnouncementScope(IQueryable<Announcement> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(a => a.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(a => a.TenantId == tenantId.Value
                                   && (a.ClassId == null || a.Class.Coaches.Any(co => co.Id == coachId.Value)))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(a => a.ClassId == null || a.Class.Students.Any(s => s.Id == studentId.Value))
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }
    private IQueryable<PaymentPlan> ApplyPaymentPlanScope(IQueryable<PaymentPlan> q)
    {
        var role = _current.Role;
        var tenantId = _current.TenantId;
        var coachId = _current.CoachId;
        var studentId = _current.StudentId;

        switch (role)
        {
            case "SuperAdmin":
                return q;

            case "Admin":
                return tenantId.HasValue
                    ? q.Where(pp => pp.Student.TenantId == tenantId.Value)
                    : q.Where(_ => false);

            case "Coach":
                return (tenantId.HasValue && coachId.HasValue)
                    ? q.Where(pp => pp.Student.TenantId == tenantId.Value
                                    && pp.Student.Class != null
                                    && pp.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
                    : q.Where(_ => false);

            case "Student":
                return studentId.HasValue
                    ? q.Where(pp => pp.StudentId == studentId.Value)
                    : q.Where(_ => false);

            default:
                return q.Where(_ => false);
        }
    }
}