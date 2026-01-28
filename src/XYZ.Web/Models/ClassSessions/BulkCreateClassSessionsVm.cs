using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.ClassSessions
{
    public sealed class BulkCreateClassSessionsVm
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public string FromDate { get; set; } = string.Empty;

        [Required]
        public string ToDate { get; set; } = string.Empty;

        [Required]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        public string EndTime { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Title { get; set; } = "Antrenman";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(120)]
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
