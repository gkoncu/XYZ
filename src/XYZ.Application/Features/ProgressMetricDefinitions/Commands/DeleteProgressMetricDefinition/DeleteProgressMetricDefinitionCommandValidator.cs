using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.DeleteProgressMetricDefinition
{
    public class DeleteProgressMetricDefinitionCommandValidator : AbstractValidator<DeleteProgressMetricDefinitionCommand>
    {
        public DeleteProgressMetricDefinitionCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçersiz metrik id.");
        }
    }
}
