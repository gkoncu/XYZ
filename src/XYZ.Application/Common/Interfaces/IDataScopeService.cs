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
        IQueryable<Student> Students();
        IQueryable<Class> Classes();
        IQueryable<Coach> Coaches();
        IQueryable<Document> Documents();
        IQueryable<Attendance> Attendances();
        IQueryable<ProgressRecord> ProgressRecords();
        IQueryable<Payment> Payments();
        IQueryable<PaymentPlan> PaymentPlans();
        IQueryable<PaymentPlan> StudentPaymentPlans(int studentId);
        IQueryable<Announcement> Announcements();

        IQueryable<Student> TenantStudents(int tenantId);
        IQueryable<Student> ClassStudents(int classId);
        IQueryable<Student> CoachStudents(int coachId);
        IQueryable<Class> CoachClasses(int coachId);

        Task<bool> CanAccessStudentAsync(int studentId, CancellationToken ct = default);
        Task<bool> CanAccessClassAsync(int classId, CancellationToken ct = default);
        Task<bool> CanAccessDocumentAsync(int documentId, CancellationToken ct = default);

        Task EnsureStudentAccessAsync(int studentId, CancellationToken ct = default);
        Task EnsureClassAccessAsync(int classId, CancellationToken ct = default);
    }
}
