using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetMyClassSessions
{
    public class GetMyClassSessionsQuery
        : IRequest<PaginationResult<ClassSessionListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public SessionStatus? Status { get; set; }

        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }

        public bool? OnlyActive { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        public string? SortBy { get; set; } = "Date";
        public string? SortDir { get; set; } = "asc";
    }
}
