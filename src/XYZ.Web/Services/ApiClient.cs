using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Json;
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
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Application.Features.Payments.Queries.GetPaymentById;
using XYZ.Application.Features.Payments.Queries.GetPayments;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Web.Models.Theming;

namespace XYZ.Web.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // === Generic HTTP ===
        public Task<HttpResponseMessage> GetAsync(
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            return _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public Task<HttpResponseMessage> PutAsJsonAsync<T>(
            string requestUri,
            T payload,
            CancellationToken cancellationToken = default)
        {
            return _httpClient.PutAsJsonAsync(requestUri, payload, cancellationToken);
        }

        public Task<HttpResponseMessage> PostAsJsonAsync<T>(
            string requestUri,
            T payload,
            CancellationToken cancellationToken = default)
        {
            return _httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);
        }

        public Task<HttpResponseMessage> DeleteAsync(
            string requestUri,
            CancellationToken cancellationToken = default)
        {
            return _httpClient.DeleteAsync(requestUri, cancellationToken);
        }

        // === Auth ===
        public async Task<LoginResultDto?> LoginAsync(
            string identifier,
            string password,
            CancellationToken cancellationToken = default)
        {
            var requestBody = new
            {
                Identifier = identifier,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("auth/login", requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResultDto>(
                cancellationToken: cancellationToken);
        }

        // === Dashboard ===
        public async Task<AdminCoachDashboardDto?> GetAdminCoachDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("dashboard/admin-coach", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AdminCoachDashboardDto>(
                cancellationToken: cancellationToken);
        }

        public async Task<StudentDashboardDto?> GetStudentDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("dashboard/student", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StudentDashboardDto>(
                cancellationToken: cancellationToken);
        }

        public async Task<SuperAdminDashboardDto?> GetSuperAdminDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("dashboard/super-admin", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<SuperAdminDashboardDto>(
                cancellationToken: cancellationToken);
        }

        // === Students ===
        public async Task<PaginationResult<StudentListItemDto>> GetStudentsAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query["SearchTerm"] = searchTerm;
            }

            var url = QueryHelpers.AddQueryString("students", query);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<StudentListItemDto>>(
                cancellationToken: cancellationToken);

            return result ?? new PaginationResult<StudentListItemDto>();
        }

        public async Task<StudentDetailDto?> GetStudentAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"students/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StudentDetailDto>(
                cancellationToken: cancellationToken);
        }

        // === Coaches ===
        public async Task<PaginationResult<CoachListItemDto>> GetCoachesAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams["SearchTerm"] = searchTerm;
            }

            var path = QueryHelpers.AddQueryString("coaches", queryParams!);

            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new PaginationResult<CoachListItemDto>
                {
                    Items = new List<CoachListItemDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            var dto = await response.Content
                .ReadFromJsonAsync<PaginationResult<CoachListItemDto>>(cancellationToken: cancellationToken);

            return dto ?? new PaginationResult<CoachListItemDto>
            {
                Items = new List<CoachListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        public async Task<CoachDetailDto?> GetCoachAsync(
            int coachId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"coaches/{coachId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CoachDetailDto>(cancellationToken: cancellationToken);
        }

        // === Admins ===
        public async Task<PaginationResult<AdminListItemDto>> GetAdminsAsync(
            string? searchTerm,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryParams["SearchTerm"] = searchTerm;
            }

            if (isActive.HasValue)
            {
                queryParams["IsActive"] = isActive.Value.ToString();
            }

            var path = QueryHelpers.AddQueryString("admins", queryParams!);

            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new PaginationResult<AdminListItemDto>
                {
                    Items = new List<AdminListItemDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            var dto = await response.Content
                .ReadFromJsonAsync<PaginationResult<AdminListItemDto>>(cancellationToken: cancellationToken);

            return dto ?? new PaginationResult<AdminListItemDto>
            {
                Items = new List<AdminListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        public async Task<AdminDetailDto?> GetAdminAsync(
            int adminId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"admins/{adminId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content
                .ReadFromJsonAsync<AdminDetailDto>(cancellationToken: cancellationToken);
        }


        // === Branches ===
        public async Task<PaginationResult<BranchListItemDto>> GetBranchesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var queryParams = new Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            var path = QueryHelpers.AddQueryString("branches", queryParams);

            var response = await _httpClient.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new PaginationResult<BranchListItemDto>
                {
                    Items = new List<BranchListItemDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }

            var dto = await response.Content
                .ReadFromJsonAsync<PaginationResult<BranchListItemDto>>(cancellationToken: cancellationToken);

            return dto ?? new PaginationResult<BranchListItemDto>
            {
                Items = new List<BranchListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };
        }

        // === Payments ===
        public async Task<PaginationResult<PaymentListItemDto>> GetPaymentsAsync(
            int? studentId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = new System.Collections.Generic.Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (studentId.HasValue)
            {
                query["StudentId"] = studentId.Value.ToString();
            }

            var url = QueryHelpers.AddQueryString("payments", query);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<PaymentListItemDto>>(
                cancellationToken: cancellationToken);

            return result ?? new PaginationResult<PaymentListItemDto>();
        }

        public async Task<PaymentDetailDto?> GetPaymentAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"payments/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PaymentDetailDto>(
                cancellationToken: cancellationToken);
        }

        public async Task<int> CreatePaymentAsync(
    CreatePaymentCommand command,
    CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("payments", command, cancellationToken);

            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return id;
        }

        public async Task<int> UpdatePaymentAsync(
            UpdatePaymentCommand command,
            CancellationToken cancellationToken = default)
        {
            if (command.Id <= 0)
            {
                throw new ArgumentException("Payment Id geçersiz.", nameof(command.Id));
            }

            var response = await _httpClient.PutAsJsonAsync($"payments/{command.Id}", command, cancellationToken);

            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return id;
        }

        public async Task<int> DeletePaymentAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"payments/{id}", cancellationToken);

            response.EnsureSuccessStatusCode();

            var deletedId = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return deletedId;
        }

        // === Payment Plans ===
        public async Task<StudentPaymentPlanDto?> GetStudentPaymentPlanAsync(
            int studentId,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"paymentplans/by-student/{studentId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StudentPaymentPlanDto>(
                cancellationToken: cancellationToken);
        }

        public async Task<int> CreatePaymentPlanAsync(
            CreatePaymentPlanCommand command,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("paymentplans", command, cancellationToken);

            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>(
                cancellationToken: cancellationToken);

            return id;
        }

        public async Task<StudentPaymentPlanDto?> GetMyPaymentPlanAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("paymentplans/my", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<StudentPaymentPlanDto>(
                cancellationToken: cancellationToken);
        }

        // === Tenant Theme ===
        public async Task<TenantThemeViewModel> GetCurrentTenantThemeAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("tenants/current-theme", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new TenantThemeViewModel();
            }

            var dto = await response.Content.ReadFromJsonAsync<
                XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme.TenantThemeDto>(
                    cancellationToken: cancellationToken);

            if (dto == null)
                return new TenantThemeViewModel();

            return new TenantThemeViewModel
            {
                Name = dto.Name,
                PrimaryColor = dto.PrimaryColor,
                SecondaryColor = dto.SecondaryColor,
                LogoUrl = dto.LogoUrl
            };
        }
    }
}
