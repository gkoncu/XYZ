using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessions
{
    public class GetClassSessionsQuery
        : IRequest<PaginationResult<ClassSessionListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Read;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int? ClassId { get; set; }
        public int? BranchId { get; set; }
        public SessionStatus? Status { get; set; }

        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }

        public bool? OnlyActive { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; } = "Date";
        public string? SortDir { get; set; } = "asc";
    }
}
