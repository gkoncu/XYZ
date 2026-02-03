using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class ProgressRecordCreateViewModel
    {
        [Required]
        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şube seçimi zorunludur.")]
        [Display(Name = "Şube")]
        public int BranchId { get; set; }

        public string? BranchName { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        [Display(Name = "Tarih")]
        public DateOnly RecordDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Koç Notu")]
        public string? CoachNotes { get; set; }

        [Display(Name = "Hedefler")]
        public string? Goals { get; set; }

        public List<ProgressRecordMetricInputViewModel> Metrics { get; set; } = new();

        public List<(int Id, string Name)> BranchOptions { get; set; } = new();
    }
}
