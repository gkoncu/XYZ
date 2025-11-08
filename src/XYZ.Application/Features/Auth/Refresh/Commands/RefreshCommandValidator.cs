using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Auth.Refresh.Commands
{
    public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken zorunludur.")
                .MaximumLength(512);
            RuleFor(x => x.UserAgent)
                .MaximumLength(256)
                .When(x => x.UserAgent is not null);
            RuleFor(x => x.CreatedByIp)
                .MaximumLength(45)
                .When(x => x.CreatedByIp is not null);
        }
    }
}
