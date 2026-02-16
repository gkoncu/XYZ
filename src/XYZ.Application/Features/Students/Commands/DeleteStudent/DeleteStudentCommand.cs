using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.DeleteStudent
{
    public sealed class DeleteStudentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.Delete;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int StudentId { get; set; }
    }
}
