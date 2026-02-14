using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Branches.Commands.UpdateBranch
{
    public sealed class UpdateBranchCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Branches.Update;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
