using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommandValidator : AbstractValidator<UpdateAdminCommand>
    {
        private static readonly PhoneAttribute PhoneAttribute = new();

        public UpdateAdminCommandValidator()
        {
            RuleFor(x => x.AdminId)
                .GreaterThan(0)
                .WithMessage("AdminId 0'dan büyük olmalıdır.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad zorunludur.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad zorunludur.")
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olmalıdır.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta zorunludur.")
                .MaximumLength(254).WithMessage("E-posta en fazla 254 karakter olmalıdır.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32).WithMessage("Telefon en fazla 32 karakter olmalıdır.")
                .Must(p => PhoneAttribute.IsValid(p))
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Telefon formatı geçersiz.");

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("T.C. Kimlik No 11 haneli ve sadece rakam olmalıdır.");
        }
    }
}
