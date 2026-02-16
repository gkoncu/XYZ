using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.Update;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int ClassId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? AgeGroupMin { get; set; }

        public int? AgeGroupMax { get; set; }

        public int MaxCapacity { get; set; }

        public int BranchId { get; set; }
    }
}
