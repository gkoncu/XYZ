using FluentValidation;

namespace XYZ.Application.Features.Auth.Register.Commands
{
    public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private static readonly string[] AllowedRoles =
        {
            "Student",
            "Coach",
            "Admin",
            "SuperAdmin",
            "Finance"
        };

        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad zorunludur.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad zorunludur.")
                .MaximumLength(100);

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role zorunludur.")
                .Must(r => AllowedRoles.Contains(r))
                .WithMessage("Geçersiz rol değeri.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("TenantId 0'dan büyük olmalıdır.");
        }
    }
}
