using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById
{
    public class GetProgressMetricDefinitionByIdQuery : IRequest<ProgressMetricDefinitionDetailDto>, IRequirePermission
    {
        public int Id { get; set; }

        public string PermissionKey => PermissionNames.ProgressMetrics.Read;
        public PermissionScope? MinimumScope => PermissionScope.Branch;
    }
}
