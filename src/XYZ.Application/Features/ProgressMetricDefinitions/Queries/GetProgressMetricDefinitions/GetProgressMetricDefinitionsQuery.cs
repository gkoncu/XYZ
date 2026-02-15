using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitions
{
    public class GetProgressMetricDefinitionsQuery : IRequest<IList<ProgressMetricDefinitionListItemDto>>, IRequirePermission
    {
        public int BranchId { get; set; }
        public bool IncludeInactive { get; set; }

        public string PermissionKey => PermissionNames.ProgressMetrics.Read;
        public PermissionScope? MinimumScope => PermissionScope.Branch;
    }
}
