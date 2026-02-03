using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.DeleteProgressMetricDefinition
{
    public class DeleteProgressMetricDefinitionCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}
