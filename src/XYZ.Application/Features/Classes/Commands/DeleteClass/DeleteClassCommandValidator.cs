using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Classes.Commands.DeleteClass
{
    public class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
    {
        public DeleteClassCommandValidator()
        {
            RuleFor(x => x.ClassId)
                .GreaterThan(0);
        }
    }
}
