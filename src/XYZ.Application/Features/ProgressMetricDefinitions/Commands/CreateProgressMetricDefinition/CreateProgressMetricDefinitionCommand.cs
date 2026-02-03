using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition
{
    public class CreateProgressMetricDefinitionCommand : IRequest<int>
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
    }
}