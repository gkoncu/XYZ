using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Branches
{
    public sealed class BranchEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        [Display(Name = "Şube Adı")]
        public string Name { get; set; } = string.Empty;
    }
}
