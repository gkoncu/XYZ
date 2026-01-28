using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Domain.Enums;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Announcements
{
    public sealed class AnnouncementUpsertViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Display(Name = "Sınıf")]
        public int? ClassId { get; set; }

        [Required(ErrorMessage = ValidationMessages.Required)]
        [MaxLength(200, ErrorMessage = ValidationMessages.MaxLength)]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [MaxLength(4000, ErrorMessage = ValidationMessages.MaxLength)]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Yayın Tarihi")]
        public DateTime PublishDate { get; set; } = DateTime.Today;

        [Display(Name = "Bitiş Tarihi")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Tür")]
        public AnnouncementType Type { get; set; } = AnnouncementType.General;

        public List<SelectListItem> TypeOptions { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PublishDate == default)
            {
                yield return new ValidationResult(
                    ValidationMessages.PublishDateRequired,
                    new[] { nameof(PublishDate) });
            }

            if (ExpiryDate.HasValue && PublishDate != default && ExpiryDate.Value.Date < PublishDate.Date)
            {
                yield return new ValidationResult(
                    ValidationMessages.ExpiryBeforePublish,
                    new[] { nameof(ExpiryDate) });
            }
        }
    }
}
