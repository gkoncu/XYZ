using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Branches
{
    public sealed class BranchCreateViewModel
    {
        [Required]
        [Display(Name = "Şube Adı")]
        public string Name { get; set; } = string.Empty;
    }
}
