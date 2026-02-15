using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.DeleteProgressMetricDefinition
{
    public class DeleteProgressMetricDefinitionCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }

        public string PermissionKey => PermissionNames.ProgressMetrics.Delete;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;
    }
}
