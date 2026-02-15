using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public sealed class CreateStudentCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Students.Create;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public string UserId { get; set; } = string.Empty;

        public int? ClassId { get; set; }

        public string? IdentityNumber { get; set; }

        public string? Address { get; set; }

        public string? Parent1FirstName { get; set; }
        public string? Parent1LastName { get; set; }
        public string? Parent1Email { get; set; }
        public string? Parent1PhoneNumber { get; set; }

        public string? Parent2FirstName { get; set; }
        public string? Parent2LastName { get; set; }
        public string? Parent2Email { get; set; }
        public string? Parent2PhoneNumber { get; set; }

        public string? Notes { get; set; }
        public string? MedicalInformation { get; set; }
    }
}
