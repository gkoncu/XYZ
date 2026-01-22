using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Admins.Queries.GetAdminById;
using XYZ.Application.Features.Admins.Queries.GetAllAdmins;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Branches.Commands.CreateBranch;
using XYZ.Application.Features.Branches.Commands.UpdateBranch;
using XYZ.Application.Features.Branches.Queries.GetAllBranches;
using XYZ.Application.Features.Branches.Queries.GetBranchById;
using XYZ.Application.Features.Classes.Commands.AssignCoachToClass;
using XYZ.Application.Features.Classes.Commands.AssignStudentToClass;
using XYZ.Application.Features.Classes.Commands.CreateClass;
using XYZ.Application.Features.Classes.Commands.UnassignCoachToClass;
using XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Application.Features.Payments.Queries.GetPaymentById;
using XYZ.Application.Features.Payments.Queries.GetPayments;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Application.Features.Tenants.Queries.GetAllTenants;
using XYZ.Application.Features.Tenants.Queries.GetTenantsById;
using XYZ.Domain.Enums;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Services
{
    public interface IApiClient
    {
        // === Generic HTTP ===
        Task<HttpResponseMessage> GetAsync(
            string requestUri,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PutAsJsonAsync<T>(
            string requestUri,
            T payload,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostAsJsonAsync<T>(
            string requestUri,
            T payload,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> DeleteAsync(
            string requestUri,
            CancellationToken cancellationToken = default);


        // === Auth ===
        Task<LoginResultDto?> LoginAsync(
            string identifier,
            string password,
            CancellationToken cancellationToken = default);

        // === Dashboard ===
        Task<AdminCoachDashboardDto?> GetAdminCoachDashboardAsync(
            CancellationToken cancellationToken = default);

        Task<StudentDashboardDto?> GetStudentDashboardAsync(
            CancellationToken cancellationToken = default);

        Task<SuperAdminDashboardDto?> GetSuperAdminDashboardAsync(
            CancellationToken cancellationToken = default);

        // === Students ===
        Task<PaginationResult<StudentListItemDto>> GetStudentsAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<StudentDetailDto?> GetStudentAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === Coaches ===
        Task<PaginationResult<CoachListItemDto>> GetCoachesAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<CoachDetailDto?> GetCoachAsync(
            int coachId,
            CancellationToken cancellationToken = default);

        // === Admins ===
        Task<PaginationResult<AdminListItemDto>> GetAdminsAsync(
            string? searchTerm,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<AdminDetailDto?> GetAdminAsync(
            int adminId,
            CancellationToken cancellationToken = default);

        // === Branches ===
        Task<PaginationResult<BranchListItemDto>> GetBranchesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<BranchDetailDto?> GetBranchAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateBranchAsync(
            CreateBranchCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdateBranchAsync(
            UpdateBranchCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeleteBranchAsync(
            int id,
            CancellationToken cancellationToken = default);


        // === Classes ===
        Task<PaginationResult<ClassListItemDto>> GetClassesAsync(
            string? searchTerm,
            int? branchId,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<ClassDetailDto?> GetClassAsync(int id, CancellationToken cancellationToken = default);

        Task<int> CreateClassAsync(CreateClassCommand command, CancellationToken cancellationToken = default);

        Task<int> DeleteClassAsync(int id, CancellationToken cancellationToken = default);

        Task<int> AssignStudentToClassAsync(int classId, AssignStudentToClassCommand command, CancellationToken cancellationToken = default);
        Task<int> UnassignStudentFromClassAsync(int classId, UnassignStudentFromClassCommand command, CancellationToken cancellationToken = default);

        Task<int> AssignCoachToClassAsync(int classId, AssignCoachToClassCommand command, CancellationToken cancellationToken = default);
        Task<int> UnassignCoachFromClassAsync(int classId, UnassignCoachFromClassCommand command, CancellationToken cancellationToken = default);


        // === Payments ===
        Task<PaginationResult<PaymentListItemDto>> GetPaymentsAsync(
            int? studentId,
            DateOnly? fromDueDate,
            DateOnly? toDueDate,
            PaymentStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PaymentDetailDto?> GetPaymentAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreatePaymentAsync(
            CreatePaymentCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdatePaymentAsync(
            UpdatePaymentCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeletePaymentAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === Payment Plans ===
        Task<StudentPaymentPlanDto?> GetStudentPaymentPlanAsync(
            int studentId,
            CancellationToken cancellationToken = default);

        Task<int> CreatePaymentPlanAsync(
            CreatePaymentPlanCommand command,
            CancellationToken cancellationToken = default);

        Task<StudentPaymentPlanDto?> GetMyPaymentPlanAsync(
            CancellationToken cancellationToken = default);

        // === Tenants (SuperAdmin) ===
        Task<PaginationResult<TenantListItemDto>> GetTenantsAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<TenantDetailDto?> GetTenantAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === Tenant Theme ===
        Task<TenantThemeViewModel> GetCurrentTenantThemeAsync(
            CancellationToken cancellationToken = default);
    }
}
