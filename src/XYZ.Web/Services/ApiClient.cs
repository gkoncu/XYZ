using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;

namespace XYZ.Web.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AdminCoachDashboardDto?> GetAdminCoachDashboardAsync(
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync("dashboard/admin-coach", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                // TODO: 401/403 için özel handling eklenecek
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AdminCoachDashboardDto>(cancellationToken: cancellationToken);
        }

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

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<StudentListItemDto>>(cancellationToken: cancellationToken);

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

            return await response.Content.ReadFromJsonAsync<StudentDetailDto>(cancellationToken: cancellationToken);
        }
    }
}
