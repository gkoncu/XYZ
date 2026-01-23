using System;
using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ProgressRecords
{
    public sealed class ProgressRecordCreateViewModel
    {
        [Required]
        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tarih")]
        public DateTime RecordDate { get; set; } = DateTime.Today;

        [Display(Name = "Boy (cm)")]
        public decimal? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "Yağ Oranı (%)")]
        public decimal? BodyFatPercentage { get; set; }

        [Display(Name = "Dikey Sıçrama")]
        public decimal? VerticalJump { get; set; }

        [Display(Name = "Sprint Süresi")]
        public decimal? SprintTime { get; set; }

        [Display(Name = "Dayanıklılık")]
        public decimal? Endurance { get; set; }

        [Display(Name = "Esneklik")]
        public decimal? Flexibility { get; set; }

        [Display(Name = "Teknik Puan")]
        public int? TechnicalScore { get; set; }

        [Display(Name = "Taktik Puan")]
        public int? TacticalScore { get; set; }

        [Display(Name = "Fiziksel Puan")]
        public int? PhysicalScore { get; set; }

        [Display(Name = "Mental Puan")]
        public int? MentalScore { get; set; }

        [Display(Name = "Koç Notu")]
        public string? CoachNotes { get; set; }

        [Display(Name = "Hedefler")]
        public string? Goals { get; set; }
    }
}
