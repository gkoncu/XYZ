using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ClassSessions
{
    public sealed class EditClassSessionVm
    {
        [Required]
        public int SessionId { get; set; }

        [Required]
        public string Date { get; set; } = string.Empty;

        [Required]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        public string EndTime { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(120)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? CoachNote { get; set; }
    }
}
