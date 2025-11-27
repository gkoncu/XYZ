using System;
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

        [Required, EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "Belirtilmedi";

        [Required]
        [Display(Name = "Kan Grubu")]
        public string BloodType { get; set; } = "Bilinmiyor";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Lisans No")]
        public string? LicenseNumber { get; set; }

        [Required]
        [Display(Name = "Branş Id")]
        public int BranchId { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }
    }
}
