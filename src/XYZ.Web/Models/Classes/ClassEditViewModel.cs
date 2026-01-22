using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Classes
{
    public sealed class ClassEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Sınıf Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Yaş Min")]
        public int? AgeGroupMin { get; set; }

        [Display(Name = "Yaş Max")]
        public int? AgeGroupMax { get; set; }

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Kontenjan")]
        public int MaxCapacity { get; set; } = 20;

        [Required]
        [Display(Name = "Şube")]
        public int BranchId { get; set; }

        [Display(Name = "Durum")]
        public bool IsActive { get; set; } = true;
    }
}
