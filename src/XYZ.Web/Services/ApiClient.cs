using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
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
