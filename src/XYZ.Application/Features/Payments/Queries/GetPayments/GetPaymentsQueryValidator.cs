using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Payments.Queries.GetPayments
{
    public class GetPaymentsQueryValidator
        : AbstractValidator<GetPaymentsQuery>
    {
        public GetPaymentsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(200);

            RuleFor(x => x.StudentId)
                .GreaterThan(0)
                .When(x => x.StudentId.HasValue);
        }
    }
}
