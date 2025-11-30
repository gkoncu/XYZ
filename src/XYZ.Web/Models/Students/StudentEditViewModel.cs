using System;
using System.ComponentModel.DataAnnotations;

namespace XYZ.Web.Models.Students
{
    public class StudentEditViewModel
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

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        [Required]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "PreferNotToSay";

        [Required]
        [Display(Name = "Kan Grubu")]
        public string BloodType { get; set; } = "Unknown";

        [Display(Name = "Sınıf")]
        public int? ClassId { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Display(Name = "Veli 1 Adı")]
        public string? Parent1FirstName { get; set; }

        [Display(Name = "Veli 1 Soyadı")]
        public string? Parent1LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Veli 1 E-posta")]
        public string? Parent1Email { get; set; }

        [Phone]
        [Display(Name = "Veli 1 Telefon")]
        public string? Parent1PhoneNumber { get; set; }

        [Display(Name = "Veli 2 Adı")]
        public string? Parent2FirstName { get; set; }

        [Display(Name = "Veli 2 Soyadı")]
        public string? Parent2LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Veli 2 E-posta")]
        public string? Parent2Email { get; set; }

        [Phone]
        [Display(Name = "Veli 2 Telefon")]
        public string? Parent2PhoneNumber { get; set; }

        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Sağlık Bilgileri")]
        public string? MedicalInformation { get; set; }

        [Display(Name = "Aktif mi?")]
        public bool IsActive { get; set; }
    }
}
