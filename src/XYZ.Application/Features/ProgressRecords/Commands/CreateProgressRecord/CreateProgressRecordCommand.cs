using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord
{
    public class CreateProgressRecordCommand : IRequest<int>
    {
        public int StudentId { get; set; }
        public int BranchId { get; set; }

        public DateOnly RecordDate { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }

        public IList<MetricValueInput> Values { get; set; } = new List<MetricValueInput>();
    }
}
