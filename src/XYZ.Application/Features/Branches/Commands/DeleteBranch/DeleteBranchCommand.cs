using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Branches.Commands.DeleteBranch
{
    public sealed class DeleteBranchCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Branches.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int BranchId { get; set; }
    }
}
