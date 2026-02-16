using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass
{
    public class UnassignStudentFromClassCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.AssignClass;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int StudentId { get; set; }
        public int ClassId { get; set; }
    }
}
