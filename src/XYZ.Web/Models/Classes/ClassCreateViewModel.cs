using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace XYZ.Web.Models.Classes
{
    public class ClassCreateViewModel
    {
        [Required(ErrorMessage = "Sınıf adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Sınıf adı en fazla 50 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir şube seçiniz.")]
        public int BranchId { get; set; }

        [Range(1, 100, ErrorMessage = "Kontenjan 1 ile 100 arasında olmalıdır.")]
        public int MaxCapacity { get; set; } = 20;

        [Range(0, 100, ErrorMessage = "Minimum yaş 0 ile 100 arasında olmalıdır.")]
        public int? AgeGroupMin { get; set; }

        [Range(0, 100, ErrorMessage = "Maksimum yaş 0 ile 0 arasında olmalıdır.")]
        public int? AgeGroupMax { get; set; }

        public List<SelectListItem> Branches { get; set; } = new();
    }
}
