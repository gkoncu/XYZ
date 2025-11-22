using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Services
{
    public interface IApiClient
    {
        // Auth
        Task<LoginResultDto?> LoginAsync(
            string identifier,
            string password,
            CancellationToken cancellationToken = default);

        // Dashboard
        Task<AdminCoachDashboardDto?> GetAdminCoachDashboardAsync(
            CancellationToken cancellationToken = default);

        // Students
        Task<PaginationResult<StudentListItemDto>> GetStudentsAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<StudentDetailDto?> GetStudentAsync(
            int id,
            CancellationToken cancellationToken = default);

        // Tenant Theme
        Task<TenantThemeViewModel> GetCurrentTenantThemeAsync(
            CancellationToken cancellationToken = default);
    }
}
