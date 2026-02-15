using MediatR;
using System;
using System.Collections.Generic;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class CreateProgressRecordCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ProgressRecords.Create;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int StudentId { get; set; }
        public int BranchId { get; set; }

        public DateOnly RecordDate { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }

        public IList<MetricValueInput> Values { get; set; } = new List<MetricValueInput>();
    }
}
