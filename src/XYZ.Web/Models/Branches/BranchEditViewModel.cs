using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Branches
{
    public sealed class BranchEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Şube Adı")]
        public string Name { get; set; } = string.Empty;
    }
}
