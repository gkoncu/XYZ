using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord
{
    public class DeleteProgressRecordCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ProgressRecords.Delete;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int Id { get; set; }
    }
}
