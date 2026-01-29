using System.ComponentModel.DataAnnotations;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Branches
{
    public sealed class BranchCreateViewModel
    {
        [Required]
        [MaxLength(30, ErrorMessage = ValidationMessages.MaxLength)]
        [Display(Name = "Şube Adı")]
        public string Name { get; set; } = string.Empty;
    }
}
