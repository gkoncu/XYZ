using FluentValidation;

namespace XYZ.Application.Features.Admins.Commands.CreateAdmin
{
    public sealed class CreateAdminCommandValidator : AbstractValidator<CreateAdminCommand>
    {
        public CreateAdminCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId zorunludur.");

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
