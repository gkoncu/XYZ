using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Commands.DeleteAdmin
{
    public sealed class DeleteAdminCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Users.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int AdminId { get; set; }
    }
}
