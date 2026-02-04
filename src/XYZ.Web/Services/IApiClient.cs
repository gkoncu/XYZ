using System.Net.Http;
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
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory;
using XYZ.Application.Features.Payments.Commands.CreatePayment;
using XYZ.Application.Features.Payments.Commands.UpdatePayment;
using XYZ.Application.Features.Payments.Queries.GetPaymentById;
using XYZ.Application.Features.Payments.Queries.GetPayments;
using XYZ.Application.Features.Profile.Commands.UpdateMyProfile;
using XYZ.Application.Features.Profile.Queries.GetMyProfile;
using XYZ.Application.Features.ProgressMetricDefinitions.Queries;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;
using XYZ.Application.Features.Tenants.Queries.GetAllTenants;
using XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme;
using XYZ.Application.Features.Tenants.Queries.GetTenantsById;
using XYZ.Domain.Enums;
using XYZ.Web.Models.Theming;
using static System.Net.WebRequestMethods;

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

        // === Account ===
        Task<bool> ForgotPasswordAsync(string email, CancellationToken ct);

        Task<(bool Ok, string? Error)> SetPasswordAsync(string userId, string token, string newPassword, CancellationToken ct);

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

        Task<int> UpdateClassAsync(int id, UpdateClassCommand command, CancellationToken cancellationToken = default);

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

        // === Documents / Compliance ===

        Task<IList<XYZ.Application.Features.Documents.Queries.DocumentStatus.StudentDocumentStatusListItemDto>> GetStudentsDocumentStatusAsync(
            bool onlyIncomplete,
            string? searchTerm,
            int take,
            CancellationToken cancellationToken = default);

        Task<IList<XYZ.Application.Features.Documents.Queries.DocumentStatus.CoachDocumentStatusListItemDto>> GetCoachesDocumentStatusAsync(
            bool onlyIncomplete,
            string? searchTerm,
            int take,
            CancellationToken cancellationToken = default);

        // === Documents / User Detail ===

        Task<XYZ.Application.Features.Documents.Queries.DocumentStatus.UserDocumentStatusDto> GetStudentDocumentStatusAsync(
            int studentId,
            CancellationToken cancellationToken = default);

        Task<XYZ.Application.Features.Documents.Queries.DocumentStatus.UserDocumentStatusDto> GetCoachDocumentStatusAsync(
            int coachId,
            CancellationToken cancellationToken = default);

        Task<IList<XYZ.Application.Features.Documents.Queries.GetUserDocuments.DocumentListItemDto>> GetStudentDocumentsAsync(
            int studentId,
            int? type,
            CancellationToken cancellationToken = default);

        Task<IList<XYZ.Application.Features.Documents.Queries.GetUserDocuments.DocumentListItemDto>> GetCoachDocumentsAsync(
            int coachId,
            int? type,
            CancellationToken cancellationToken = default);

        Task<int> UploadStudentDocumentAsync(
            int studentId,
            Stream fileStream,
            string fileName,
            string? name,
            string? description,
            int documentDefinitionId,
            CancellationToken cancellationToken = default);

        Task<int> UploadCoachDocumentAsync(
            int coachId,
            Stream fileStream,
            string fileName,
            string? name,
            string? description,
            int documentDefinitionId,
            CancellationToken cancellationToken = default);

        Task<int> DeleteDocumentAsync(int id, CancellationToken cancellationToken = default);

        // === Document Definitions ===
        Task<IList<XYZ.Application.Features.DocumentDefinitions.Queries.DocumentDefinitionListItemDto>> GetDocumentDefinitionsAsync(
            int target,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        Task<int> CreateDocumentDefinitionAsync(
            XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition.CreateDocumentDefinitionCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdateDocumentDefinitionAsync(
            int id,
            XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition.UpdateDocumentDefinitionCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeleteDocumentDefinitionAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === Documents / Download ===
        Task<(Stream Stream, string ContentType, string FileName)> DownloadDocumentAsync(int id, CancellationToken cancellationToken = default);


        // === Payment Plans ===
        Task<StudentPaymentPlanDto?> GetStudentPaymentPlanAsync(
            int studentId,
            CancellationToken cancellationToken = default);

        Task<int> CreatePaymentPlanAsync(
            CreatePaymentPlanCommand command,
            CancellationToken cancellationToken = default);

        Task<StudentPaymentPlanDto?> GetMyPaymentPlanAsync(
            CancellationToken cancellationToken = default);

        Task<IList<PaymentPlanHistoryItemDto>> GetStudentPaymentPlanHistoryAsync(
            int studentId,
            CancellationToken cancellationToken = default);

        Task<StudentPaymentPlanDto?> GetPaymentPlanDetailsAsync(
            int planId,
            CancellationToken cancellationToken = default);

        Task<int> CancelPaymentPlanAsync(
            int planId,
            CancellationToken cancellationToken = default);

        Task<int> ArchivePaymentPlanAsync(
            int planId,
            CancellationToken cancellationToken = default);


        // === ProgressRecords ===
        Task<IList<ProgressRecordListItemDto>> GetStudentProgressRecordsAsync(
            int studentId,
            DateOnly? from,
            DateOnly? to,
            int? branchId,
            CancellationToken cancellationToken = default);

        Task<ProgressRecordDetailDto?> GetProgressRecordAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateProgressRecordAsync(
            CreateProgressRecordCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdateProgressRecordAsync(
            int id,
            UpdateProgressRecordCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeleteProgressRecordAsync(
            int id,
            CancellationToken cancellationToken = default);

        // === ProgressMetricDefinitions ===
        Task<IList<ProgressMetricDefinitionListItemDto>> GetProgressMetricDefinitionsAsync(
            int branchId,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        Task<int> CreateProgressMetricDefinitionAsync(
            XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition.CreateProgressMetricDefinitionCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdateProgressMetricDefinitionAsync(
            int id,
            XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition.UpdateProgressMetricDefinitionCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeleteProgressMetricDefinitionAsync(
            int id,
            CancellationToken cancellationToken = default);


        // === Announcements ===
        Task<PaginationResult<AnnouncementListItemDto>> GetAnnouncementsAsync(
            string? searchTerm,
            int? classId,
            AnnouncementType? type,
            bool onlyCurrent,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<AnnouncementDetailDto?> GetAnnouncementAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAnnouncementAsync(
            CreateAnnouncementCommand command,
            CancellationToken cancellationToken = default);

        Task<int> UpdateAnnouncementAsync(
            int id,
            UpdateAnnouncementCommand command,
            CancellationToken cancellationToken = default);

        Task<int> DeleteAnnouncementAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> BroadcastSystemAnnouncementAsync(
            CreateSystemAnnouncementForAllTenantsCommand command,
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

        // === Profile ===
        Task<MyProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateMyProfileAsync(UpdateMyProfileCommand command, CancellationToken cancellationToken = default);
        Task<(bool Ok, string? ErrorCode)> ChangeMyPasswordAsync( string currentPassword, string newPassword, CancellationToken cancellationToken = default);
        Task<string?> UploadMyProfilePictureAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);


        Task<TenantThemeDto?> GetCurrentTenantThemeRawAsync(CancellationToken cancellationToken = default);
        Task<bool> UpdateCurrentTenantThemeAsync(UpdateCurrentTenantThemeCommand command, CancellationToken cancellationToken = default);

    }
}
