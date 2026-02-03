using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById
{
    namespace XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById
    {
        public class GetProgressMetricDefinitionByIdQueryValidator : AbstractValidator<GetProgressMetricDefinitionByIdQuery>
        {
            public GetProgressMetricDefinitionByIdQueryValidator()
            {
                RuleFor(x => x.Id)
                    .GreaterThan(0).WithMessage("Geçersiz metrik id.");
            }
        }
    }
}
