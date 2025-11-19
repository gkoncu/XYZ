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
        public DateTime RecordDate { get; set; }

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BodyFatPercentage { get; set; }

        public int? TechnicalScore { get; set; }
        public int? TacticalScore { get; set; }
        public int? PhysicalScore { get; set; }
        public int? MentalScore { get; set; }

        public string? CoachNotes { get; set; }
    }
}
