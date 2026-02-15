using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class GetProgressRecordByIdQuery : IRequest<ProgressRecordDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ProgressRecords.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
    }
}
