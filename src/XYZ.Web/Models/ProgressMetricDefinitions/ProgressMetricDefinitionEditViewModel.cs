using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;

namespace XYZ.Web.Models.ProgressMetricDefinitions
{
    public sealed class ProgressMetricDefinitionEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public int BranchId { get; set; }

        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Metrik adı zorunludur.")]
        [MaxLength(120)]
        [Display(Name = "Metrik Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Veri Tipi")]
        public ProgressMetricDataType DataType { get; set; }

        [MaxLength(32)]
        [Display(Name = "Birim (kg, cm, sn...)")]
        public string? Unit { get; set; }

        [Display(Name = "Zorunlu")]
        public bool IsRequired { get; set; }

        [Display(Name = "Sıralama")]
        [Range(0, 10000)]
        public int SortOrder { get; set; } = 100;

        [Display(Name = "Min Değer")]
        public decimal? MinValue { get; set; }

        [Display(Name = "Max Değer")]
        public decimal? MaxValue { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
}
