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
        IQueryable<Student> GetScopedStudents();
        IQueryable<Class> GetScopedClasses();
        IQueryable<Coach> GetScopedCoaches();
        IQueryable<Document> GetScopedDocuments();
        IQueryable<Attendance> GetScopedAttendances();
        IQueryable<ProgressRecord> GetScopedProgressRecords();
        IQueryable<Payment> GetScopedPayments();
        IQueryable<Announcement> GetScopedAnnouncements();

        Task<bool> CanAccessStudentAsync(int studentId);
        Task<bool> CanAccessClassAsync(int classId);
        Task<bool> CanAccessDocumentAsync(int documentId);
    }
}
