using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition
{
    public class CreateProgressMetricDefinitionCommand : IRequest<int>, IRequirePermission
    {
        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        public bool IsActive { get; set; } = true;

        public string PermissionKey => PermissionNames.ProgressMetrics.Create;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;
    }
}
