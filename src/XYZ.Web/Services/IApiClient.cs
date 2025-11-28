using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Admins.Queries.GetAdminById;
using XYZ.Application.Features.Admins.Queries.GetAllAdmins;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Branches.Queries.GetAllBranches;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Application.Features.Payments.Queries.GetPaymentById;
using XYZ.Application.Features.Payments.Queries.GetPayments;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
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

        // === Payments ===
        Task<PaginationResult<PaymentListItemDto>> GetPaymentsAsync(
            int? studentId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<PaymentDetailDto?> GetPaymentAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === Tenant Theme ===
        Task<TenantThemeViewModel> GetCurrentTenantThemeAsync(
            CancellationToken cancellationToken = default);
    }
}
