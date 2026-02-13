using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Classes.Commands.AssignStudentToClass
{
    public class AssignStudentToClassCommand : IRequest<int>, IRequirePermission
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }

        public string PermissionKey => PermissionNames.Students.AssignClass;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;
    }
}
