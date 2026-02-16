using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Queries.GetAdminById
{
    public sealed class GetAdminByIdQuery : IRequest<AdminDetailDto?>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Users.Read;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int AdminId { get; set; }
    }
}
