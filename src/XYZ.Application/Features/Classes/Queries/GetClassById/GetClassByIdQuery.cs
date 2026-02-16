using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Queries.GetClassById
{
    public class GetClassByIdQuery : IRequest<ClassDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Classes.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int ClassId { get; set; }
    }
}
