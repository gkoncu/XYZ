using MediatR;
using System;
using System.Collections.Generic;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class GetStudentProgressRecordsQuery
        : IRequest<IList<ProgressRecordListItemDto>>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ProgressRecords.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int StudentId { get; set; }
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public int? BranchId { get; set; }
    }
}
