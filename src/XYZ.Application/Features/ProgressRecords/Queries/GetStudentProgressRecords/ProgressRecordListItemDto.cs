using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords
{
    public class ProgressRecordListItemDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateOnly RecordDate { get; set; }
        public int Sequence { get; set; }

        public string? CreatedByDisplayName { get; set; }
        public int FilledMetricsCount { get; set; }
    }
}
