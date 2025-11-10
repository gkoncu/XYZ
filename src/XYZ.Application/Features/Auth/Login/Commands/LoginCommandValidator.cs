using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Auth.Login.Commands
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Identifier (email/phone) zorunludur.")
                .MaximumLength(256);
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.")
                .MinimumLength(4);
            RuleFor(x => x.UserAgent)
                .MaximumLength(256)
                .When(x => x.UserAgent is not null);
            RuleFor(x => x.CreatedByIp)
                .MaximumLength(45)
                .When(x => x.CreatedByIp is not null);
        }
    }
}
