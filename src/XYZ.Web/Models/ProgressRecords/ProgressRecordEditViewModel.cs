using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class ProgressRecordEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = string.Empty;

        public int BranchId { get; set; }
        public string? BranchName { get; set; }

        public DateOnly RecordDate { get; set; }
        public int Sequence { get; set; }

        public string? CreatedByDisplayName { get; set; }

        [Display(Name = "Koç Notu")]
        public string? CoachNotes { get; set; }

        [Display(Name = "Hedefler")]
        public string? Goals { get; set; }

        public List<ProgressRecordMetricInputViewModel> Metrics { get; set; } = new();
    }
}
