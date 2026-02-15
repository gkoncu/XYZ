using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition
{
    public class UpdateProgressMetricDefinitionCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public int BranchId { get; set; }

        public string Name { get; set; } = string.Empty;
        public ProgressMetricDataType DataType { get; set; }
        public string? Unit { get; set; }

        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        public bool IsActive { get; set; }

        public string PermissionKey => PermissionNames.ProgressMetrics.Update;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;
    }
}
