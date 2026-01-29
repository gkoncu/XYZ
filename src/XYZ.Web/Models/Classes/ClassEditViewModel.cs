using System.ComponentModel.DataAnnotations;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Classes
{
    public sealed class ClassEditViewModel
    {
        [Required(ErrorMessage = ValidationMessages.Required)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Sınıf Adı")]
        [StringLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = ValidationMessages.MaxLength)]
        public string? Description { get; set; }

        [Display(Name = "Yaş Min")]
        [Range(0, 100, ErrorMessage = "Minimum yaş 0 ile 100 arasında olmalıdır.")]
        public int? AgeGroupMin { get; set; }

        [Display(Name = "Yaş Max")]
        [Range(0, 100, ErrorMessage = "Maksimum yaş 0 ile 0 arasında olmalıdır.")]
        public int? AgeGroupMax { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Kontenjan 1 ile 100 arasında olmalıdır.")]
        [Display(Name = "Kontenjan")]
        public int MaxCapacity { get; set; } = 20;

        [Required]
        [Display(Name = "Şube")]
        public int BranchId { get; set; }

        [Display(Name = "Durum")]
        public bool IsActive { get; set; } = true;
    }
}
