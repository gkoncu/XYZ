using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Branches.Commands.CreateBranch
{
    public sealed class CreateBranchCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Branches.Create;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public string Name { get; set; } = string.Empty;
    }
}