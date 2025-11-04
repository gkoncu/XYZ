using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Common.Interfaces
{
    public interface IDataScopeService
    {
        IQueryable<Student> ApplyStudentScope(IQueryable<Student> query, ApplicationUser currentUser);
        IQueryable<Class> ApplyClassScope(IQueryable<Class> query, ApplicationUser currentUser);
        IQueryable<Document> ApplyDocumentScope(IQueryable<Document> query, ApplicationUser currentUser);
        IQueryable<Attendance> ApplyAttendanceScope(IQueryable<Attendance> query, ApplicationUser currentUser);
        IQueryable<ProgressRecord> ApplyProgressRecordScope(IQueryable<ProgressRecord> query, ApplicationUser currentUser);
        IQueryable<Payment> ApplyPaymentScope(IQueryable<Payment> query, ApplicationUser currentUser);
        IQueryable<Announcement> ApplyAnnouncementScope(IQueryable<Announcement> query, ApplicationUser currentUser);

        //Scoped queries
        IQueryable<Student> GetScopedStudents();
        IQueryable<Coach> GetScopedCoaches();
        IQueryable<Class> GetScopedClasses();

        Task<bool> CanAccessStudentAsync(int studentId, ApplicationUser currentUser);
        Task<bool> CanAccessClassAsync(int classId, ApplicationUser currentUser);
        Task<bool> CanAccessDocumentAsync(int documentId, ApplicationUser currentUser);
        Task<bool> CanAccessAttendanceAsync(int attendanceId, ApplicationUser currentUser);
    }
}
