using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById
{
    public class GetProgressMetricDefinitionByIdQuery : IRequest<ProgressMetricDefinitionDetailDto>
    {
        public int Id { get; set; }
    }
}
