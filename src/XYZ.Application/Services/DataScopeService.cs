using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Services;

public class DataScopeService : IDataScopeService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;
    private readonly IPermissionService _permissions;

    private int? _cachedCoachBranchId;
    private bool _coachBranchIdLoaded;

    public DataScopeService(IApplicationDbContext context, ICurrentUserService currentUser, IPermissionService permissions)
    {
        _context = context;
        _current = currentUser;
        _permissions = permissions;
    }

    // -------- Scoped Root Queries --------
    public IQueryable<Student> Students() => ApplyStudentScope(_context.Students);
    public IQueryable<Class> Classes() => ApplyClassScope(_context.Classes);
    public IQueryable<Coach> Coaches() => ApplyCoachScope(_context.Coaches);
    public IQueryable<Document> Documents() => ApplyDocumentScope(_context.Documents);
    public IQueryable<Attendance> Attendances() => ApplyAttendanceScope(_context.Attendances);
    public IQueryable<ProgressRecord> ProgressRecords() => ApplyProgressScope(_context.ProgressRecords);
    public IQueryable<Payment> Payments() => ApplyPaymentScope(_context.Payments);
    public IQueryable<PaymentPlan> PaymentPlans() => ApplyPaymentPlanScope(_context.PaymentPlans);
    public IQueryable<Announcement> Announcements() => ApplyAnnouncementScope(_context.Announcements);
    public IQueryable<Branch> Branches() => ApplyBranchScope(_context.Branches);

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

    public async Task<bool> CanAccessBranchAsync(int branchId, CancellationToken ct = default)
        => await Branches().AnyAsync(b => b.Id == branchId, ct);

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

    public async Task EnsureBranchAccessAsync(int branchId, CancellationToken ct = default)
    {
        if (!await CanAccessBranchAsync(branchId, ct))
            throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");
    }

    // -------- Inside Scope (Row-level WHERE) --------

    private IQueryable<Student> ApplyStudentScope(IQueryable<Student> q)
    {
        var scope = ResolveAny(PermissionNames.Students.Read, PermissionNames.Students.Create, PermissionNames.Students.Update, PermissionNames.Students.Archive, PermissionNames.Students.Delete, PermissionNames.Students.AssignClass, PermissionNames.Students.ChangeClass);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyStudentBranchScope(q),
            PermissionScope.OwnClasses => ApplyStudentOwnClassesScope(q),
            PermissionScope.Self => ApplyStudentSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Student> ApplyStudentSelfScope(IQueryable<Student> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue ? q.Where(s => s.Id == studentId.Value) : q.Where(_ => false);
    }

    private IQueryable<Student> ApplyStudentOwnClassesScope(IQueryable<Student> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(s => s.Class != null && s.Class.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Student> ApplyStudentBranchScope(IQueryable<Student> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(s => s.Class != null && s.Class.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Class> ApplyClassScope(IQueryable<Class> q)
    {
        var scope = ResolveAny(PermissionNames.Classes.Read, PermissionNames.ClassSessions.Read, PermissionNames.ClassSessions.Create, PermissionNames.ClassSessions.Update, PermissionNames.ClassSessions.ChangeStatus, PermissionNames.ClassSessions.Delete);

        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyClassBranchScope(q),
            PermissionScope.OwnClasses => ApplyClassOwnClassesScope(q),
            PermissionScope.Self => ApplyClassSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Class> ApplyClassSelfScope(IQueryable<Class> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue
            ? q.Where(c => c.Students.Any(s => s.Id == studentId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Class> ApplyClassOwnClassesScope(IQueryable<Class> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(c => c.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Class> ApplyClassBranchScope(IQueryable<Class> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(c => c.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Coach> ApplyCoachScope(IQueryable<Coach> q)
    {
        var scope = ResolveAny(PermissionNames.Coaches.Read);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyCoachBranchScope(q),
            PermissionScope.OwnClasses => ApplyCoachSelfScope(q),
            PermissionScope.Self => ApplyCoachSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Coach> ApplyCoachSelfScope(IQueryable<Coach> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue ? q.Where(c => c.Id == coachId.Value) : q.Where(_ => false);
    }

    private IQueryable<Coach> ApplyCoachBranchScope(IQueryable<Coach> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(c => c.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Branch> ApplyBranchScope(IQueryable<Branch> q)
    {
        var scope = ResolveAny(
            PermissionNames.Branches.Read, 
            PermissionNames.Branches.Update, 
            PermissionNames.Branches.Delete, 
            PermissionNames.Branches.Archive);

        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyBranchBranchScope(q),
            PermissionScope.OwnClasses => ApplyBranchBranchScope(q),
            PermissionScope.Self => ApplyBranchBranchScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Branch> ApplyBranchBranchScope(IQueryable<Branch> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(b => b.Id == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Attendance> ApplyAttendanceScope(IQueryable<Attendance> q)
    {
        var scope = ResolveAny(PermissionNames.Attendance.Read, PermissionNames.Students.AttendanceRead);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyAttendanceBranchScope(q),
            PermissionScope.OwnClasses => ApplyAttendanceOwnClassesScope(q),
            PermissionScope.Self => ApplyAttendanceSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Attendance> ApplyAttendanceSelfScope(IQueryable<Attendance> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue ? q.Where(a => a.StudentId == studentId.Value) : q.Where(_ => false);
    }

    private IQueryable<Attendance> ApplyAttendanceOwnClassesScope(IQueryable<Attendance> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(a => a.Class.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Attendance> ApplyAttendanceBranchScope(IQueryable<Attendance> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(a => a.Class.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Payment> ApplyPaymentScope(IQueryable<Payment> q)
    {
        var scope = ResolveAny(PermissionNames.Payments.Read, PermissionNames.Students.PaymentsRead);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyPaymentBranchScope(q),
            PermissionScope.OwnClasses => ApplyPaymentOwnClassesScope(q),
            PermissionScope.Self => ApplyPaymentSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Payment> ApplyPaymentSelfScope(IQueryable<Payment> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue ? q.Where(p => p.StudentId == studentId.Value) : q.Where(_ => false);
    }

    private IQueryable<Payment> ApplyPaymentOwnClassesScope(IQueryable<Payment> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(p => p.Student.Class != null && p.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Payment> ApplyPaymentBranchScope(IQueryable<Payment> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(p => p.Student.Class != null && p.Student.Class.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<PaymentPlan> ApplyPaymentPlanScope(IQueryable<PaymentPlan> q)
    {
        var scope = ResolveAny(PermissionNames.Payments.Read, PermissionNames.Students.PaymentsRead);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyPaymentPlanBranchScope(q),
            PermissionScope.OwnClasses => ApplyPaymentPlanOwnClassesScope(q),
            PermissionScope.Self => ApplyPaymentPlanSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<PaymentPlan> ApplyPaymentPlanSelfScope(IQueryable<PaymentPlan> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue ? q.Where(pp => pp.StudentId == studentId.Value) : q.Where(_ => false);
    }

    private IQueryable<PaymentPlan> ApplyPaymentPlanOwnClassesScope(IQueryable<PaymentPlan> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(pp => pp.Student.Class != null && pp.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<PaymentPlan> ApplyPaymentPlanBranchScope(IQueryable<PaymentPlan> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(pp => pp.Student.Class != null && pp.Student.Class.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<Document> ApplyDocumentScope(IQueryable<Document> q)
    {
        var scope = ResolveAny(PermissionNames.Documents.Read, PermissionNames.Students.DocumentsRead);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyDocumentBranchScope(q),
            PermissionScope.OwnClasses => ApplyDocumentOwnClassesScope(q),
            PermissionScope.Self => ApplyDocumentSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Document> ApplyDocumentSelfScope(IQueryable<Document> q)
    {
        var studentId = _current.StudentId;
        if (studentId.HasValue) return q.Where(d => d.StudentId == studentId.Value);

        var coachId = _current.CoachId;
        if (coachId.HasValue) return q.Where(d => d.CoachId == coachId.Value);

        return q.Where(_ => false);
    }

    private IQueryable<Document> ApplyDocumentOwnClassesScope(IQueryable<Document> q)
    {
        var coachId = _current.CoachId;
        if (!coachId.HasValue) return q.Where(_ => false);

        var cid = coachId.Value;

        return q.Where(d =>
            (d.CoachId.HasValue && d.CoachId.Value == cid)
            || (d.StudentId.HasValue
                && d.Student.Class != null
                && d.Student.Class.Coaches.Any(co => co.Id == cid))
        );
    }

    private IQueryable<Document> ApplyDocumentBranchScope(IQueryable<Document> q)
    {
        var branchId = GetCurrentCoachBranchId();
        if (!branchId.HasValue) return q.Where(_ => false);

        var bid = branchId.Value;

        return q.Where(d =>
            (d.CoachId.HasValue && d.Coach.BranchId == bid)
            || (d.StudentId.HasValue
                && d.Student.Class != null
                && d.Student.Class.BranchId == bid)
        );
    }

    private IQueryable<Announcement> ApplyAnnouncementScope(IQueryable<Announcement> q)
    {
        var scope = ResolveAny(PermissionNames.Announcements.Read, PermissionNames.Announcements.ReadPublic);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyAnnouncementBranchScope(q),
            PermissionScope.OwnClasses => ApplyAnnouncementOwnClassesScope(q),
            PermissionScope.Self => ApplyAnnouncementSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<Announcement> ApplyAnnouncementSelfScope(IQueryable<Announcement> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue
            ? q.Where(a => a.ClassId == null || a.Class.Students.Any(s => s.Id == studentId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Announcement> ApplyAnnouncementOwnClassesScope(IQueryable<Announcement> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(a => a.ClassId == null || a.Class.Coaches.Any(c => c.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<Announcement> ApplyAnnouncementBranchScope(IQueryable<Announcement> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(a => a.ClassId == null || a.Class.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    private IQueryable<ProgressRecord> ApplyProgressScope(IQueryable<ProgressRecord> q)
    {
        var scope = ResolveAny(PermissionNames.ProgressRecords.Read, PermissionNames.ProgressRecords.Create, PermissionNames.ProgressRecords.Update, PermissionNames.ProgressRecords.Delete);
        if (scope is null) return q.Where(_ => false);

        return scope.Value switch
        {
            PermissionScope.AllTenants => q,
            PermissionScope.Tenant => q,
            PermissionScope.Branch => ApplyProgressBranchScope(q),
            PermissionScope.OwnClasses => ApplyProgressOwnClassesScope(q),
            PermissionScope.Self => ApplyProgressSelfScope(q),
            _ => q.Where(_ => false)
        };
    }

    private IQueryable<ProgressRecord> ApplyProgressSelfScope(IQueryable<ProgressRecord> q)
    {
        var studentId = _current.StudentId;
        return studentId.HasValue ? q.Where(r => r.StudentId == studentId.Value) : q.Where(_ => false);
    }

    private IQueryable<ProgressRecord> ApplyProgressOwnClassesScope(IQueryable<ProgressRecord> q)
    {
        var coachId = _current.CoachId;
        return coachId.HasValue
            ? q.Where(r => r.Student.Class != null && r.Student.Class.Coaches.Any(co => co.Id == coachId.Value))
            : q.Where(_ => false);
    }

    private IQueryable<ProgressRecord> ApplyProgressBranchScope(IQueryable<ProgressRecord> q)
    {
        var branchId = GetCurrentCoachBranchId();
        return branchId.HasValue
            ? q.Where(r => r.BranchId == branchId.Value)
            : q.Where(_ => false);
    }

    // -------- Permission helpers --------

    private PermissionScope? ResolveScope(string permissionKey)
        => _permissions.GetScopeAsync(permissionKey).GetAwaiter().GetResult();

    private PermissionScope? ResolveAny(params string[] permissionKeys)
    {
        PermissionScope? best = null;

        foreach (var key in permissionKeys)
        {
            var scope = ResolveScope(key);
            if (scope is null) continue;

            if (best is null || scope.Value > best.Value)
                best = scope;
        }

        return best;
    }

    private int? GetCurrentCoachBranchId()
    {
        if (_coachBranchIdLoaded)
            return _cachedCoachBranchId;

        _coachBranchIdLoaded = true;

        var coachId = _current.CoachId;
        if (!coachId.HasValue)
            return _cachedCoachBranchId = null;

        _cachedCoachBranchId = _context.Coaches
            .AsNoTracking()
            .Where(c => c.Id == coachId.Value)
            .Select(c => (int?)c.BranchId)
            .FirstOrDefault();

        return _cachedCoachBranchId;
    }
}
