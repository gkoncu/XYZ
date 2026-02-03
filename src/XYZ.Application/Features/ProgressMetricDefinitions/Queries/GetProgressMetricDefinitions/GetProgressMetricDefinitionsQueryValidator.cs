using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitions
{
    public class GetProgressMetricDefinitionsQueryValidator : AbstractValidator<GetProgressMetricDefinitionsQuery>
    {
        public GetProgressMetricDefinitionsQueryValidator()
        {
            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Şube seçimi zorunludur.");
        }
    }
}
