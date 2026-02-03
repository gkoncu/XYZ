using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitions
{
    public class GetProgressMetricDefinitionsQuery : IRequest<IList<ProgressMetricDefinitionListItemDto>>
    {
        public int BranchId { get; set; }
        public bool IncludeInactive { get; set; }
    }
}
