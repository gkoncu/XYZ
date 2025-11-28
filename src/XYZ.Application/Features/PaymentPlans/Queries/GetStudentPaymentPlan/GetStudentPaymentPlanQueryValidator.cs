using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan
{
    public class GetStudentPaymentPlanQueryValidator : AbstractValidator<GetStudentPaymentPlanQuery>
    {
        public GetStudentPaymentPlanQueryValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0);
        }
    }
}
