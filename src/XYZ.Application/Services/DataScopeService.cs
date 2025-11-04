using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Services;

public class DataScopeService : IDataScopeService
{
    private readonly IApplicationDbContext _context;

    public DataScopeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Student> ApplyStudentScope(IQueryable<Student> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(s => s.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(s => s.TenantId == currentUser.TenantId &&
                                              (s.Class.HeadCoach.UserId == currentUser.Id ||
                                               s.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(s => s.UserId == currentUser.Id),
            _ => query.Where(s => false)
        };
    }

    public IQueryable<Class> ApplyClassScope(IQueryable<Class> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(c => c.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(c => c.TenantId == currentUser.TenantId &&
                                              (c.HeadCoach.UserId == currentUser.Id ||
                                               c.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(c => c.TenantId == currentUser.TenantId &&
                                               c.Students.Any(s => s.UserId == currentUser.Id)),
            _ => query.Where(c => false)
        };
    }

    public IQueryable<Document> ApplyDocumentScope(IQueryable<Document> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(d => d.Student.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(d => d.Student.TenantId == currentUser.TenantId &&
                                              (d.Student.Class.HeadCoach.UserId == currentUser.Id ||
                                               d.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(d => d.Student.UserId == currentUser.Id),
            _ => query.Where(d => false)
        };
    }

    public IQueryable<Attendance> ApplyAttendanceScope(IQueryable<Attendance> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(a => a.Student.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(a => a.Student.TenantId == currentUser.TenantId &&
                                              (a.Student.Class.HeadCoach.UserId == currentUser.Id ||
                                               a.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(a => a.Student.UserId == currentUser.Id),
            _ => query.Where(a => false)
        };
    }

    public IQueryable<ProgressRecord> ApplyProgressRecordScope(IQueryable<ProgressRecord> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(p => p.Student.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(p => p.Student.TenantId == currentUser.TenantId &&
                                              (p.Student.Class.HeadCoach.UserId == currentUser.Id ||
                                               p.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(p => p.Student.UserId == currentUser.Id),
            _ => query.Where(p => false)
        };
    }

    public IQueryable<Payment> ApplyPaymentScope(IQueryable<Payment> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(p => p.Student.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(p => p.Student.TenantId == currentUser.TenantId &&
                                              (p.Student.Class.HeadCoach.UserId == currentUser.Id ||
                                               p.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id))),
            UserRole.Student => query.Where(p => p.Student.UserId == currentUser.Id),
            _ => query.Where(p => false)
        };
    }

    public IQueryable<Announcement> ApplyAnnouncementScope(IQueryable<Announcement> query, ApplicationUser currentUser)
    {
        return currentUser.Role switch
        {
            UserRole.SuperAdmin => query,
            UserRole.Admin => query.Where(a => a.TenantId == currentUser.TenantId),
            UserRole.Coach => query.Where(a => a.TenantId == currentUser.TenantId &&
                                              (a.ClassId == null ||
                                               (a.Class.HeadCoach.UserId == currentUser.Id ||
                                                a.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id)))),
            UserRole.Student => query.Where(a => a.TenantId == currentUser.TenantId &&
                                               (a.ClassId == null ||
                                                a.Class.Students.Any(s => s.UserId == currentUser.Id))),
            _ => query.Where(a => false)
        };
    }

    public async Task<bool> CanAccessStudentAsync(int studentId, ApplicationUser currentUser)
    {
        var student = await _context.Students
            .Include(s => s.Class)
                .ThenInclude(c => c.HeadCoach)
            .Include(s => s.Class)
                .ThenInclude(c => c.AssistantCoaches)
                    .ThenInclude(ac => ac.Coach)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null) return false;

        return currentUser.Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => student.TenantId == currentUser.TenantId,
            UserRole.Coach => student.TenantId == currentUser.TenantId &&
                             (student.Class.HeadCoach.UserId == currentUser.Id ||
                              student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id)),
            UserRole.Student => student.UserId == currentUser.Id,
            _ => false
        };
    }

    public async Task<bool> CanAccessClassAsync(int classId, ApplicationUser currentUser)
    {
        var classEntity = await _context.Classes
            .Include(c => c.HeadCoach)
            .Include(c => c.AssistantCoaches)
                .ThenInclude(ac => ac.Coach)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (classEntity == null) return false;

        return currentUser.Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => classEntity.TenantId == currentUser.TenantId,
            UserRole.Coach => classEntity.TenantId == currentUser.TenantId &&
                             (classEntity.HeadCoach.UserId == currentUser.Id ||
                              classEntity.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id)),
            UserRole.Student => classEntity.Students.Any(s => s.UserId == currentUser.Id),
            _ => false
        };
    }

    public async Task<bool> CanAccessDocumentAsync(int documentId, ApplicationUser currentUser)
    {
        var document = await _context.Documents
            .Include(d => d.Student)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.HeadCoach)
            .Include(d => d.Student)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.AssistantCoaches)
                        .ThenInclude(ac => ac.Coach)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null) return false;

        return currentUser.Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => document.Student.TenantId == currentUser.TenantId,
            UserRole.Coach => document.Student.TenantId == currentUser.TenantId &&
                             (document.Student.Class.HeadCoach.UserId == currentUser.Id ||
                              document.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id)),
            UserRole.Student => document.Student.UserId == currentUser.Id,
            _ => false
        };
    }

    public async Task<bool> CanAccessAttendanceAsync(int attendanceId, ApplicationUser currentUser)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.HeadCoach)
            .Include(a => a.Student)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.AssistantCoaches)
                        .ThenInclude(ac => ac.Coach)
            .FirstOrDefaultAsync(a => a.Id == attendanceId);

        if (attendance == null) return false;

        return currentUser.Role switch
        {
            UserRole.SuperAdmin => true,
            UserRole.Admin => attendance.Student.TenantId == currentUser.TenantId,
            UserRole.Coach => attendance.Student.TenantId == currentUser.TenantId &&
                             (attendance.Student.Class.HeadCoach.UserId == currentUser.Id ||
                              attendance.Student.Class.AssistantCoaches.Any(ac => ac.Coach.UserId == currentUser.Id)),
            UserRole.Student => attendance.Student.UserId == currentUser.Id,
            _ => false
        };

    }



}
