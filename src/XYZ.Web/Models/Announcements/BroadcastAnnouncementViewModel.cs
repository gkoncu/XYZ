using System;
using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Announcements
{
    public sealed class BroadcastAnnouncementViewModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Yayın Tarihi")]
        public DateTime PublishDate { get; set; } = DateTime.Today;

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ExpiryDate { get; set; }
    }
}
