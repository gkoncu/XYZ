using MediatR;
using System.Collections.Generic;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class UpdateProgressRecordCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ProgressRecords.Update;
        public PermissionScope? MinimumScope => PermissionScope.OwnClasses;

        public int Id { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }

        public IList<MetricValueInput> Values { get; set; } = new List<MetricValueInput>();
    }
}
