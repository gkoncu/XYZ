using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Coaches
{
    public class CoachEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Branş")]
        public string? Branch { get; set; }

        [Display(Name = "Hakkında / Özgeçmiş")]
        public string? Bio { get; set; }

        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }
    }
}
