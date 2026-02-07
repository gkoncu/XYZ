using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;
using XYZ.Web.Infrastructure;

namespace XYZ.Web.Models.Admins
{
    public sealed class AdminCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Ad")]
        [StringLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Soyad")]
        [StringLength(50, ErrorMessage = ValidationMessages.MaxLength)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidationMessages.Required)]
        [EmailAddress(ErrorMessage = ValidationMessages.Email)]
        [Display(Name = "E-posta")]
        [StringLength(256, ErrorMessage = ValidationMessages.MaxLength)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = ValidationMessages.Phone)]
        [Display(Name = "Telefon")]
        [StringLength(20, ErrorMessage = ValidationMessages.MaxLength)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = ValidationMessages.Required)]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Kan Grubu")]
        public string BloodType { get; set; } = "Unknown";

        [Required(ErrorMessage = ValidationMessages.Required)]
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; } = "PreferNotToSay";

        public int? TenantId { get; set; }

        [Display(Name = "T.C. Kimlik No")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = ValidationMessages.TcIdentity)]
        public string? IdentityNumber { get; set; }

        [Display(Name = "Kullanıcı Yönetimi Yetkisi")]
        public bool CanManageUsers { get; set; } = true;

        [Display(Name = "Finans Yönetimi Yetkisi")]
        public bool CanManageFinance { get; set; } = true;

        [Display(Name = "Ayarlar Yönetimi Yetkisi")]
        public bool CanManageSettings { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BirthDate.HasValue)
            {
                var today = DateTime.Today;
                var bd = BirthDate.Value.Date;

                var min = today.AddYears(-100);
                var max = today.AddYears(-16);

                if (bd > today || bd < min || bd > max)
                    yield return new ValidationResult(ValidationMessages.BirthDateRangeAdmin, new[] { nameof(BirthDate) });
            }

            if (!Enum.TryParse<BloodType>(BloodType, ignoreCase: true, out _))
                yield return new ValidationResult(ValidationMessages.BloodTypeInvalid, new[] { nameof(BloodType) });

            if (!Enum.TryParse<Gender>(Gender, ignoreCase: true, out _))
                yield return new ValidationResult(ValidationMessages.GenderInvalid, new[] { nameof(Gender) });
        }
    }
}
