using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Queries.GetAllAdmins
{
    public sealed class GetAllAdminsQuery : IRequest<PaginationResult<AdminListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Users.Read;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
