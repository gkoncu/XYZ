using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Audit.Queries.GetAuditEventById
{
    public sealed class GetAuditEventByIdQuery
        : IRequest<AuditEventDetailDto?>, IRequirePermission
    {
        public int Id { get; set; }

        public string PermissionKey => PermissionNames.Audit.ReadTenant;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;
    }
}
