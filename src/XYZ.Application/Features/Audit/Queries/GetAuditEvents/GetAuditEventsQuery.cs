using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEvents
{
    public sealed class GetAuditEventsQuery
        : IRequest<PaginationResult<AuditEventListItemDto>>, IRequirePermission
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? EntityName { get; set; }
        public string? EntityKey { get; set; }
        public AuditAction? Action { get; set; }
        public string? ActorUserId { get; set; }

        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }

        public string PermissionKey => PermissionNames.Audit.ReadTenant;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;
    }
}
