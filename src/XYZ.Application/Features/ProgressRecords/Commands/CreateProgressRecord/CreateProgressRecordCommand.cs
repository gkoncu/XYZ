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

        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

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
