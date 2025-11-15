using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Commands.UpdateBranch
{
    public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
    {
        public UpdateBranchCommandValidator()
        {
            RuleFor(x => x.BranchId)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
