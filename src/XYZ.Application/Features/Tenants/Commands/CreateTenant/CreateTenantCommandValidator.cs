using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
    {
        public CreateTenantCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Subdomain)
                .NotEmpty()
                .MaximumLength(100)
                .Matches("^[a-z0-9-]+$")
                .WithMessage("Subdomain sadece küçük harf, rakam ve '-' karakterlerinden oluşmalıdır.");

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Phone)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.PrimaryColor)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PrimaryColor));

            RuleFor(x => x.SecondaryColor)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.SecondaryColor));

            RuleFor(x => x)
                .Must(x =>
                {
                    if (x.SubscriptionStartDate.HasValue && x.SubscriptionEndDate.HasValue)
                        return x.SubscriptionEndDate > x.SubscriptionStartDate;
                    return true;
                })
                .WithMessage("SubscriptionEndDate, SubscriptionStartDate tarihinden büyük olmalıdır.");
        }
    }
}
