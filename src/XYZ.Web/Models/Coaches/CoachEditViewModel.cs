using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;

namespace XYZ.Web.Models.Coaches
{
    public class CoachEditViewModel : IValidatableObject
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçersiz koç.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "E-posta formatı geçersiz.")]
        [StringLength(256, ErrorMessage = "E-posta en fazla 256 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefon numarası geçerli değil.")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Cinsiyet zorunludur.")]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "PreferNotToSay";

        [Required(ErrorMessage = "Kan grubu zorunludur.")]
        [Display(Name = "Kan Grubu")]
        public string BloodType { get; set; } = "Unknown";

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.")]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Lisans No")]
        [StringLength(50, ErrorMessage = "Lisans no en fazla 50 karakter olabilir.")]
        public string? LicenseNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir şube seçiniz.")]
        [Display(Name = "Şube")]
        public int BranchId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!BirthDate.HasValue)
                yield break;

            var today = DateTime.Today;
            var bd = BirthDate.Value.Date;

            if (bd > today)
            {
                yield return new ValidationResult("Doğum tarihi gelecek bir tarih olamaz.", new[] { nameof(BirthDate) });
                yield break;
            }

            var minAllowed = today.AddYears(-100);
            var maxAllowed = today.AddYears(-16);

            if (bd < minAllowed)
                yield return new ValidationResult("Doğum tarihi 100 yaş üstünü desteklemez.", new[] { nameof(BirthDate) });

            if (bd > maxAllowed)
                yield return new ValidationResult("Koç yaşı en az 16 olmalıdır.", new[] { nameof(BirthDate) });

            if (!Enum.TryParse<Gender>(Gender, ignoreCase: true, out _))
                yield return new ValidationResult("Cinsiyet değeri geçersiz.", new[] { nameof(Gender) });

            if (!Enum.TryParse<BloodType>(BloodType, ignoreCase: true, out _))
                yield return new ValidationResult("Kan grubu değeri geçersiz.", new[] { nameof(BloodType) });
        }
    }
}
