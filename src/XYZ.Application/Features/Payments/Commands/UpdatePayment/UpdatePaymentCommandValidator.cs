using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommandValidator
        : AbstractValidator<UpdatePaymentCommand>
    {
        public UpdatePaymentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DiscountAmount.HasValue);
        }
    }
}
