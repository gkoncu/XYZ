using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Admins.Queries.GetAdminById;
using XYZ.Application.Features.Admins.Queries.GetAllAdmins;
using XYZ.Application.Features.Announcements.Commands.CreateAnnouncement;
using XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants;
using XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement;
using XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements;
using XYZ.Application.Features.Announcements.Queries.GetAnnouncementById;
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
using XYZ.Application.Features.Classes.Commands.UpdateClass;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Queries;
using XYZ.Application.Features.Documents.Queries.DocumentStatus;
using XYZ.Application.Features.Documents.Queries.GetUserDocuments;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Application.Features.Payments.Queries.GetPaymentById;
using XYZ.Application.Features.Payments.Queries.GetPayments;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Application.Features.Tenants.Queries.GetAllTenants;
using XYZ.Application.Features.Tenants.Queries.GetTenantsById;
using XYZ.Domain.Enums;
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

        // === Classes ===
        public async Task<PaginationResult<ClassListItemDto>> GetClassesAsync(
            string? searchTerm,
            int? branchId,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm)) query["SearchTerm"] = searchTerm;
            if (branchId.HasValue) query["BranchId"] = branchId.Value.ToString();
            if (isActive.HasValue) query["IsActive"] = isActive.Value.ToString();

            var url = Microsoft.AspNetCore.WebUtilities.QueryHelpers.AddQueryString("classes", query);

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<PaginationResult<ClassListItemDto>>(cancellationToken: cancellationToken)
                   ?? new PaginationResult<ClassListItemDto>();
        }

        public async Task<ClassDetailDto?> GetClassAsync(int id, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.GetAsync($"classes/{id}", cancellationToken);
            if (!resp.IsSuccessStatusCode) return null;

            return await resp.Content.ReadFromJsonAsync<ClassDetailDto>(cancellationToken: cancellationToken);
        }

        public async Task<int> CreateClassAsync(CreateClassCommand command, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("classes", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> UpdateClassAsync(int id, UpdateClassCommand command, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"classes/{id}", command, cancellationToken);
            response.EnsureSuccessStatusCode();

            var updatedId = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return updatedId;
        }

        public async Task<int> DeleteClassAsync(int id, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.DeleteAsync($"classes/{id}", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> AssignStudentToClassAsync(int classId, AssignStudentToClassCommand command, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync($"classes/{classId}/assign-student", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> UnassignStudentFromClassAsync(int classId, UnassignStudentFromClassCommand command, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync($"classes/{classId}/unassign-student", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> AssignCoachToClassAsync(int classId, AssignCoachToClassCommand command, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync($"classes/{classId}/assign-coach", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> UnassignCoachFromClassAsync(int classId, UnassignCoachFromClassCommand command, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync($"classes/{classId}/unassign-coach", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        // === Documents / Compliance Index ===
        public async Task<IList<StudentDocumentStatusListItemDto>> GetStudentsDocumentStatusAsync(
            bool onlyIncomplete,
            string? searchTerm,
            int take,
            CancellationToken cancellationToken = default)
        {
            var url = $"documents/students/status?onlyIncomplete={onlyIncomplete.ToString().ToLowerInvariant()}&take={take}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                url += $"&searchTerm={Uri.EscapeDataString(searchTerm.Trim())}";

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<IList<StudentDocumentStatusListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<StudentDocumentStatusListItemDto>();
        }

        public async Task<IList<CoachDocumentStatusListItemDto>> GetCoachesDocumentStatusAsync(
            bool onlyIncomplete,
            string? searchTerm,
            int take,
            CancellationToken cancellationToken = default)
        {
            var url = $"documents/coaches/status?onlyIncomplete={onlyIncomplete.ToString().ToLowerInvariant()}&take={take}";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                url += $"&searchTerm={Uri.EscapeDataString(searchTerm.Trim())}";

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<IList<CoachDocumentStatusListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<CoachDocumentStatusListItemDto>();
        }

        // === Documents / User Detail Status ===
        public async Task<UserDocumentStatusDto> GetStudentDocumentStatusAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.GetAsync($"documents/student/{studentId}/status", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<UserDocumentStatusDto>(cancellationToken: cancellationToken))
                   ?? new UserDocumentStatusDto();
        }

        public async Task<UserDocumentStatusDto> GetCoachDocumentStatusAsync(int coachId, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.GetAsync($"documents/coach/{coachId}/status", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<UserDocumentStatusDto>(cancellationToken: cancellationToken))
                   ?? new UserDocumentStatusDto();
        }

        // === Documents / Lists ===
        public async Task<IList<DocumentListItemDto>> GetStudentDocumentsAsync(int studentId, int? type, CancellationToken cancellationToken = default)
        {
            var url = $"documents/student/{studentId}";
            if (type.HasValue) url += $"?type={type.Value}";

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<IList<DocumentListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<DocumentListItemDto>();
        }

        public async Task<IList<DocumentListItemDto>> GetCoachDocumentsAsync(int coachId, int? type, CancellationToken cancellationToken = default)
        {
            var url = $"documents/coach/{coachId}";
            if (type.HasValue) url += $"?type={type.Value}";

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<IList<DocumentListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<DocumentListItemDto>();
        }

        // === Documents / Upload ===
        public async Task<int> UploadStudentDocumentAsync(
            int studentId,
            Stream fileStream,
            string fileName,
            string? name,
            string? description,
            int documentDefinitionId,
            CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "File", fileName);

            if (!string.IsNullOrWhiteSpace(name))
                content.Add(new StringContent(name.Trim()), "Name");

            if (!string.IsNullOrWhiteSpace(description))
                content.Add(new StringContent(description.Trim()), "Description");

            content.Add(new StringContent(documentDefinitionId.ToString()), "DocumentDefinitionId");

            var resp = await _httpClient.PostAsync($"documents/student/{studentId}", content, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        public async Task<int> UploadCoachDocumentAsync(
            int coachId,
            Stream fileStream,
            string fileName,
            string? name,
            string? description,
            int documentDefinitionId,
            CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "File", fileName);

            if (!string.IsNullOrWhiteSpace(name))
                content.Add(new StringContent(name.Trim()), "Name");

            if (!string.IsNullOrWhiteSpace(description))
                content.Add(new StringContent(description.Trim()), "Description");

            content.Add(new StringContent(documentDefinitionId.ToString()), "DocumentDefinitionId");

            var resp = await _httpClient.PostAsync($"documents/coach/{coachId}", content, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        // === Documents / Delete ===
        public async Task<int> DeleteDocumentAsync(int id, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.DeleteAsync($"documents/{id}", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        // === Document Definitions ===
        public async Task<IList<DocumentDefinitionListItemDto>> GetDocumentDefinitionsAsync(
            int target,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var url = $"documentdefinitions?target={target}&includeInactive={includeInactive.ToString().ToLowerInvariant()}";

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<IList<DocumentDefinitionListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<DocumentDefinitionListItemDto>();
        }

        public async Task<int> CreateDocumentDefinitionAsync(
            CreateDocumentDefinitionCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("documentdefinitions", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        public async Task<int> UpdateDocumentDefinitionAsync(
            int id,
            UpdateDocumentDefinitionCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PutAsJsonAsync($"documentdefinitions/{id}", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        public async Task<int> DeleteDocumentDefinitionAsync(int id, CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.DeleteAsync($"documentdefinitions/{id}", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return (await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken));
        }

        // === Payments ===
        public async Task<PaginationResult<PaymentListItemDto>> GetPaymentsAsync(
            int? studentId,
            DateOnly? fromDueDate,
            DateOnly? toDueDate,
            PaymentStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var url = "payments";

            var query = new Dictionary<string, string?>()
            {
                ["studentId"] = studentId?.ToString(),
                ["fromDueDate"] = fromDueDate?.ToString("yyyy-MM-dd"),
                ["toDueDate"] = toDueDate?.ToString("yyyy-MM-dd"),
                ["status"] = status?.ToString(),
                ["pageNumber"] = pageNumber.ToString(),
                ["pageSize"] = pageSize.ToString()
            };

            var finalUrl = QueryHelpers.AddQueryString(url, query);

            var result = await _httpClient.GetFromJsonAsync<PaginationResult<PaymentListItemDto>>(finalUrl, cancellationToken);
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

        public async Task<BranchDetailDto?> GetBranchAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"branches/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<BranchDetailDto>(cancellationToken: cancellationToken);
        }

        public async Task<int> CreateBranchAsync(
            CreateBranchCommand command,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("branches", command, cancellationToken);
            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return id;
        }

        public async Task<int> UpdateBranchAsync(
            UpdateBranchCommand command,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"branches/{command.BranchId}", command, cancellationToken);
            response.EnsureSuccessStatusCode();

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            return id;
        }

        public async Task<int> DeleteBranchAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"branches/{id}", cancellationToken);
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

        // === ProgressRecords ===
        public async Task<IList<ProgressRecordListItemDto>> GetStudentProgressRecordsAsync(
            int studentId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var url = $"progressrecords/student/{studentId}";

            var query = new Dictionary<string, string?>();
            if (from.HasValue) query["from"] = from.Value.ToString("yyyy-MM-dd");
            if (to.HasValue) query["to"] = to.Value.ToString("yyyy-MM-dd");

            if (query.Count > 0)
            {
                url = QueryHelpers.AddQueryString(url, query);
            }

            var resp = await _httpClient.GetAsync(url, cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var contentType = resp.Content.Headers.ContentType?.ToString() ?? "-";
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);

                var message =
                    $"Gelişim kayıtları alınamadı. " +
                    $"HTTP {(int)resp.StatusCode} ({resp.ReasonPhrase}), " +
                    $"Url: {url}, Content-Type: {contentType}, Body: {body}";

                throw new InvalidOperationException(message);
            }

            return (await resp.Content.ReadFromJsonAsync<IList<ProgressRecordListItemDto>>(cancellationToken: cancellationToken))
                   ?? new List<ProgressRecordListItemDto>();
        }

        public async Task<ProgressRecordDetailDto?> GetProgressRecordAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.GetAsync($"progressrecords/{id}", cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            return await resp.Content.ReadFromJsonAsync<ProgressRecordDetailDto>(cancellationToken: cancellationToken);
        }

        public async Task<int> CreateProgressRecordAsync(
            CreateProgressRecordCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("progressrecords", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> UpdateProgressRecordAsync(
            int id,
            UpdateProgressRecordCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PutAsJsonAsync($"progressrecords/{id}", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> DeleteProgressRecordAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.DeleteAsync($"progressrecords/{id}", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        // === Announcements ===
        public async Task<PaginationResult<AnnouncementListItemDto>> GetAnnouncementsAsync(
            string? searchTerm,
            int? classId,
            AnnouncementType? type,
            bool onlyCurrent,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var url = "announcements";

            var query = new Dictionary<string, string?>
            {
                ["OnlyCurrent"] = onlyCurrent.ToString(),
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm)) query["SearchTerm"] = searchTerm.Trim();
            if (classId.HasValue) query["ClassId"] = classId.Value.ToString();
            if (type.HasValue) query["Type"] = type.Value.ToString();

            url = QueryHelpers.AddQueryString(url, query);

            var resp = await _httpClient.GetAsync(url, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<PaginationResult<AnnouncementListItemDto>>(cancellationToken: cancellationToken)
                   ?? new PaginationResult<AnnouncementListItemDto>();
        }

        public async Task<AnnouncementDetailDto?> GetAnnouncementAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.GetAsync($"announcements/{id}", cancellationToken);
            if (!resp.IsSuccessStatusCode)
                return null;

            return await resp.Content.ReadFromJsonAsync<AnnouncementDetailDto>(cancellationToken: cancellationToken);
        }

        public async Task<int> CreateAnnouncementAsync(
            CreateAnnouncementCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("announcements", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> UpdateAnnouncementAsync(
            int id,
            UpdateAnnouncementCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PutAsJsonAsync($"announcements/{id}", command, cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> DeleteAnnouncementAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.DeleteAsync($"announcements/{id}", cancellationToken);
            resp.EnsureSuccessStatusCode();

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }

        public async Task<int> BroadcastSystemAnnouncementAsync(
            CreateSystemAnnouncementForAllTenantsCommand command,
            CancellationToken cancellationToken = default)
        {
            var resp = await _httpClient.PostAsJsonAsync("announcements/system/broadcast", command, cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                var msg = string.IsNullOrWhiteSpace(body)
                    ? $"Sistem duyurusu gönderilemedi. (HTTP {(int)resp.StatusCode})"
                    : body;

                throw new InvalidOperationException(msg);
            }

            return await resp.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
        }


        // === Tenants (SuperAdmin) ===
        public async Task<PaginationResult<TenantListItemDto>> GetTenantsAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                ["PageNumber"] = pageNumber.ToString(),
                ["PageSize"] = pageSize.ToString()
            };

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query["SearchTerm"] = searchTerm;
            }

            var url = QueryHelpers.AddQueryString("tenants", query);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PaginationResult<TenantListItemDto>>(
                cancellationToken: cancellationToken);

            return result ?? new PaginationResult<TenantListItemDto>();
        }

        public async Task<TenantDetailDto?> GetTenantAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"tenants/{id}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TenantDetailDto>(
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
