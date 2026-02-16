using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.DeleteClass
{
    public class DeleteClassCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Branch;

        public int ClassId { get; set; }
    }
}
