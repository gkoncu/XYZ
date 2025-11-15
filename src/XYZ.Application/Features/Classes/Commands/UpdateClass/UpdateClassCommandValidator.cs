using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.UpdateClass
{
    public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
    {
        public UpdateClassCommandValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.BranchId)
                .GreaterThan(0);

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0)
                .WithMessage("MaxCapacity 0'dan büyük olmalıdır.");

            RuleFor(x => x.AgeGroupMin)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AgeGroupMin.HasValue);

            RuleFor(x => x.AgeGroupMax)
                .GreaterThanOrEqualTo(0)
                .When(x => x.AgeGroupMax.HasValue);

            RuleFor(x => x)
                .Must(x =>
                    !x.AgeGroupMin.HasValue ||
                    !x.AgeGroupMax.HasValue ||
                    x.AgeGroupMin.Value <= x.AgeGroupMax.Value)
                .WithMessage("AgeGroupMin, AgeGroupMax değerinden büyük olamaz.");
        }
    }
}
