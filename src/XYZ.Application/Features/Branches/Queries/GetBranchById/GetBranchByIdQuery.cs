using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Branches.Queries.GetBranchById
{
    public sealed class GetBranchByIdQuery : IRequest<BranchDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Branches.Read;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int BranchId { get; set; }
    }
}
