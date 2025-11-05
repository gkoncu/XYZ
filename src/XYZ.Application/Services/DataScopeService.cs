using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Services;

public class DataScopeService : IDataScopeService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DataScopeService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public IQueryable<Student> GetScopedStudents()
    {
        var query = _context.Students
            .Include(s => s.Class)
                .ThenInclude(c => c.Coaches)
            .Include(s => s.User)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Class> GetScopedClasses()
    {
        var query = _context.Classes
            .Include(c => c.Coaches)
            .Include(c => c.Students)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Coach> GetScopedCoaches()
    {
        var query = _context.Coaches
            .Include(c => c.Classes)
            .Include(c => c.User)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Document> GetScopedDocuments()
    {
        var query = _context.Documents
            .Include(d => d.Student)
                .ThenInclude(s => s.Class)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Attendance> GetScopedAttendances()
    {
        var query = _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.Class)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<ProgressRecord> GetScopedProgressRecords()
    {
        var query = _context.ProgressRecords
            .Include(p => p.Student)
                .ThenInclude(s => s.Class)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Payment> GetScopedPayments()
    {
        var query = _context.Payments
            .Include(p => p.Student)
                .ThenInclude(s => s.Class)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    public IQueryable<Announcement> GetScopedAnnouncements()
    {
        var query = _context.Announcements
            .Include(a => a.Class)
            .AsQueryable();

        return ApplyDataScoping(query);
    }

    private IQueryable<T> ApplyDataScoping<T>(IQueryable<T> query) where T : class
    {
        var user = _currentUserService;
        if (user == null) return query;

        return user.Role switch
        {
            "SuperAdmin" => query,
            "Admin" => ApplyAdminScoping(query),
            "Coach" => ApplyCoachScoping(query),
            "Student" => ApplyStudentScoping(query),
            _ => query.Where(x => false)
        };
    }

    private IQueryable<T> ApplyAdminScoping<T>(IQueryable<T> query)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == null) return query.Where(x => false);

        if (typeof(T) == typeof(Student))
        {
            return query.Cast<Student>()
                .Where(s => s.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Class))
        {
            return query.Cast<Class>()
                .Where(c => c.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Coach))
        {
            return query.Cast<Coach>()
                .Where(c => c.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Document))
        {
            return query.Cast<Document>()
                .Where(d => d.Student.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Attendance))
        {
            return query.Cast<Attendance>()
                .Where(a => a.Student.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(ProgressRecord))
        {
            return query.Cast<ProgressRecord>()
                .Where(p => p.Student.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Payment))
        {
            return query.Cast<Payment>()
                .Where(p => p.Student.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Announcement))
        {
            return query.Cast<Announcement>()
                .Where(a => a.TenantId == tenantId)
                .Cast<T>();
        }

        return query;
    }

    private IQueryable<T> ApplyCoachScoping<T>(IQueryable<T> query)
    {
        var coachId = _currentUserService.CoachId;
        var tenantId = _currentUserService.TenantId;

        if (coachId == null || tenantId == null) return query.Where(x => false);

        if (typeof(T) == typeof(Student))
        {
            return query.Cast<Student>()
                .Where(s => s.TenantId == tenantId &&
                           s.Class != null &&
                           s.Class.Coaches.Any(c => c.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Class))
        {
            return query.Cast<Class>()
                .Where(c => c.TenantId == tenantId &&
                           c.Coaches.Any(co => co.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Coach))
        {
            return query.Cast<Coach>()
                .Where(c => c.Id == coachId && c.TenantId == tenantId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Document))
        {
            return query.Cast<Document>()
                .Where(d => d.Student.TenantId == tenantId &&
                           d.Student.Class != null &&
                           d.Student.Class.Coaches.Any(c => c.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Attendance))
        {
            return query.Cast<Attendance>()
                .Where(a => a.Student.TenantId == tenantId &&
                           a.Student.Class != null &&
                           a.Student.Class.Coaches.Any(c => c.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(ProgressRecord))
        {
            return query.Cast<ProgressRecord>()
                .Where(p => p.Student.TenantId == tenantId &&
                           p.Student.Class != null &&
                           p.Student.Class.Coaches.Any(c => c.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Payment))
        {
            return query.Cast<Payment>()
                .Where(p => p.Student.TenantId == tenantId &&
                           p.Student.Class != null &&
                           p.Student.Class.Coaches.Any(c => c.Id == coachId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Announcement))
        {
            return query.Cast<Announcement>()
                .Where(a => a.TenantId == tenantId &&
                           (a.ClassId == null ||
                            a.Class.Coaches.Any(c => c.Id == coachId)))
                .Cast<T>();
        }

        return query;
    }

    private IQueryable<T> ApplyStudentScoping<T>(IQueryable<T> query)
    {
        var studentId = _currentUserService.StudentId;
        if (studentId == null) return query.Where(x => false);

        if (typeof(T) == typeof(Student))
        {
            return query.Cast<Student>()
                .Where(s => s.Id == studentId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Document))
        {
            return query.Cast<Document>()
                .Where(d => d.StudentId == studentId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Attendance))
        {
            return query.Cast<Attendance>()
                .Where(a => a.StudentId == studentId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(ProgressRecord))
        {
            return query.Cast<ProgressRecord>()
                .Where(p => p.StudentId == studentId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Payment))
        {
            return query.Cast<Payment>()
                .Where(p => p.StudentId == studentId)
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Class))
        {
            return query.Cast<Class>()
                .Where(c => c.Students.Any(s => s.Id == studentId))
                .Cast<T>();
        }
        else if (typeof(T) == typeof(Announcement))
        {
            return query.Cast<Announcement>()
                .Where(a => a.ClassId == null ||
                           a.Class.Students.Any(s => s.Id == studentId))
                .Cast<T>();
        }

        return query;
    }

    public async Task<bool> CanAccessStudentAsync(int studentId)
    {
        var student = await GetScopedStudents()
            .FirstOrDefaultAsync(s => s.Id == studentId);

        return student != null;
    }

    public async Task<bool> CanAccessClassAsync(int classId)
    {
        var classEntity = await GetScopedClasses()
            .FirstOrDefaultAsync(c => c.Id == classId);

        return classEntity != null;
    }

    public async Task<bool> CanAccessDocumentAsync(int documentId)
    {
        var document = await GetScopedDocuments()
            .FirstOrDefaultAsync(d => d.Id == documentId);

        return document != null;
    }
}