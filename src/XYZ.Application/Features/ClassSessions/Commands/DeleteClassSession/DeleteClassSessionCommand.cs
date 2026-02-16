using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Commands.DeleteClassSession
{
    public class DeleteClassSessionCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Delete;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int Id { get; set; }
    }
}
