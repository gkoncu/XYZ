using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ClassSessions
{
    public sealed class EditClassSessionVm
    {
        [Required(ErrorMessage = "Geçersiz seans.")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public string Date { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public string EndTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(50, ErrorMessage = "Başlık en fazla 50 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [StringLength(80, ErrorMessage = "Konum en fazla 80 karakter olabilir.")]
        public string? Location { get; set; }

        [StringLength(500, ErrorMessage = "Koç notu en fazla 500 karakter olabilir.")]
        public string? CoachNote { get; set; }
    }
}
