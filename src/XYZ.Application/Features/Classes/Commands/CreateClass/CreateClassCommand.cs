using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.CreateClass
{
    public class CreateClassCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.Create;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? AgeGroupMin { get; set; }

        public int? AgeGroupMax { get; set; }

        public int MaxCapacity { get; set; }

        public int BranchId { get; set; }
    }
}
