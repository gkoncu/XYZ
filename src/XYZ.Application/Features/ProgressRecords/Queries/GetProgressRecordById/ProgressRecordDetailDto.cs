using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class ProgressRecordDetailDto
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public DateTime RecordDate { get; set; }

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? VerticalJump { get; set; }
        public decimal? SprintTime { get; set; }
        public decimal? Endurance { get; set; }
        public decimal? Flexibility { get; set; }

        public int? TechnicalScore { get; set; }
        public int? TacticalScore { get; set; }
        public int? PhysicalScore { get; set; }
        public int? MentalScore { get; set; }

        public string? CoachNotes { get; set; }
        public string? Goals { get; set; }
    }
}
