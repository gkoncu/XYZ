using FluentValidation;
using System.ComponentModel.DataAnnotations;
using XYZ.API.Controllers;

namespace XYZ.API.Validators
{
    public sealed class CreateAdminRequestValidator : AbstractValidator<AdminsController.CreateAdminRequest>
    {
        private static readonly PhoneAttribute Phone = new();

        public CreateAdminRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Ad alanı zorunludur.")
                .MaximumLength(50)
                .WithMessage("Ad en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Soyad alanı zorunludur.")
                .MaximumLength(50)
                .WithMessage("Soyad en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("E-posta alanı zorunludur.")
                .MaximumLength(254)
                .WithMessage("E-posta en fazla 254 karakter olmalıdır.")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(32)
                .WithMessage("Telefon en fazla 32 karakter olmalıdır.")
                .Must(v => Phone.IsValid(v))
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Telefon formatı geçersiz.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .When(x => x.TenantId.HasValue)
                .WithMessage("TenantId 0'dan büyük olmalıdır.");

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.");
        }
    }
}
