using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Branches.Queries.GetAllBranches
{
    public class GetAllBranchesQueryValidator : AbstractValidator<GetAllBranchesQuery>
    {
        public GetAllBranchesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(200);
        }
    }
}
