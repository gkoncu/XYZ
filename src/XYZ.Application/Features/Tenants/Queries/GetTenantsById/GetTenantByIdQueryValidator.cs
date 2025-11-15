using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetTenantsById
{
    public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
    {
        public GetTenantByIdQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0);
        }
    }
}
