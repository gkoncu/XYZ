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
        IQueryable<Announcement> Announcements();
        IQueryable<Branch> Branches();

        IQueryable<PaymentPlan> StudentPaymentPlans(int studentId);

        IQueryable<Student> TenantStudents(int tenantId);
        IQueryable<Student> ClassStudents(int classId);
        IQueryable<Student> CoachStudents(int coachId);
        IQueryable<Class> CoachClasses(int coachId);

        Task<bool> CanAccessStudentAsync(int studentId, CancellationToken ct = default);
        Task<bool> CanAccessClassAsync(int classId, CancellationToken ct = default);
        Task<bool> CanAccessDocumentAsync(int documentId, CancellationToken ct = default);
        Task<bool> CanAccessBranchAsync(int branchId, CancellationToken ct = default);

        Task EnsureStudentAccessAsync(int studentId, CancellationToken ct = default);
        Task EnsureClassAccessAsync(int classId, CancellationToken ct = default);
        Task EnsureBranchAccessAsync(int branchId, CancellationToken ct = default);
    }
}
