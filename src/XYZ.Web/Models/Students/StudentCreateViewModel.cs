using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;

namespace XYZ.Web.Models.Students
{
    public class StudentCreateViewModel : IValidatableObject
    {
        private const int NameMax = 50;
        private const int EmailMax = 256;
        private const int PhoneMax = 20;
        private const int AddressMax = 500;
        private const int NotesMax = 2000;

        [Required, StringLength(NameMax, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(NameMax, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(EmailMax, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(PhoneMax, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "PreferNotToSay";

        [Required(ErrorMessage = "Kan grubu zorunludur.")]
        [Display(Name = "Kan Grubu")]
        public string BloodType { get; set; } = "Unknown";

        [Display(Name = "Sınıf")]
        public int? ClassId { get; set; }

        public List<ClassListItemDto> ClassOptions { get; set; } = new();

        [Display(Name = "T.C. Kimlik No")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Adres")]
        [StringLength(AddressMax, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
        public string? Address { get; set; }

        [Display(Name = "Veli 1 Adı")]
        [StringLength(NameMax, ErrorMessage = "Veli adı en fazla 50 karakter olabilir.")]
        public string? Parent1FirstName { get; set; }

        [Display(Name = "Veli 1 Soyadı")]
        [StringLength(NameMax, ErrorMessage = "Veli soyadı en fazla 50 karakter olabilir.")]
        public string? Parent1LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(EmailMax, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
        [Display(Name = "Veli 1 E-posta")]
        public string? Parent1Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(PhoneMax, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Veli 1 Telefon")]
        public string? Parent1PhoneNumber { get; set; }

        [Display(Name = "Veli 2 Adı")]
        [StringLength(NameMax, ErrorMessage = "Veli adı en fazla 50 karakter olabilir.")]
        public string? Parent2FirstName { get; set; }

        [Display(Name = "Veli 2 Soyadı")]
        [StringLength(NameMax, ErrorMessage = "Veli soyadı en fazla 50 karakter olabilir.")]
        public string? Parent2LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(EmailMax, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
        [Display(Name = "Veli 2 E-posta")]
        public string? Parent2Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(PhoneMax, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Veli 2 Telefon")]
        public string? Parent2PhoneNumber { get; set; }

        [Display(Name = "Notlar")]
        [StringLength(NotesMax, ErrorMessage = "Notlar en fazla 2000 karakter olabilir.")]
        public string? Notes { get; set; }

        [Display(Name = "Sağlık Bilgileri")]
        [StringLength(NotesMax, ErrorMessage = "Sağlık bilgileri en fazla 2000 karakter olabilir.")]
        public string? MedicalInformation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!BirthDate.HasValue)
                yield break;

            var today = DateTime.Today;
            var min = today.AddYears(-100);

            if (BirthDate.Value.Date > today)
                yield return new ValidationResult("Doğum tarihi gelecekte olamaz.", new[] { nameof(BirthDate) });

            if (BirthDate.Value.Date < min)
                yield return new ValidationResult("Doğum tarihi 100 yıldan eski olamaz.", new[] { nameof(BirthDate) });
        }
    }
}
