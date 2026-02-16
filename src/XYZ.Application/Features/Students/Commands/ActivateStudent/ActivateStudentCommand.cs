using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.ActivateStudent
{
    public sealed class ActivateStudentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.Archive;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int StudentId { get; set; }
    }
}
