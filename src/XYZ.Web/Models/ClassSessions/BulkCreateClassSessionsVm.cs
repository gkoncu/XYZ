using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ClassSessions
{
    public sealed class BulkCreateClassSessionsVm
    {
        [Required(ErrorMessage = "Lütfen bir sınıf seçiniz.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public string FromDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public string ToDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(50, ErrorMessage = "Başlık en fazla 50 karakter olabilir.")]
        public string Title { get; set; } = "Antrenman";

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [StringLength(80, ErrorMessage = "Konum en fazla 80 karakter olabilir.")]
        public string? Location { get; set; }

        public bool GenerateAttendance { get; set; } = true;

        public bool Monday { get; set; } = true;
        public bool Tuesday { get; set; } = true;
        public bool Wednesday { get; set; } = true;
        public bool Thursday { get; set; } = true;
        public bool Friday { get; set; } = true;
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
}
