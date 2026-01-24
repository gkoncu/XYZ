using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Domain.Enums;

namespace XYZ.Web.Models.Announcements
{
    public sealed class AnnouncementUpsertViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Sınıf")]
        public int? ClassId { get; set; }

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

        [Display(Name = "Tür")]
        public AnnouncementType Type { get; set; } = AnnouncementType.General;

        public List<SelectListItem> TypeOptions { get; set; } = new();
    }
}
