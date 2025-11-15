using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Commands.DeleteTenant
{
    public class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
    {
        public DeleteTenantCommandValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0);
        }
    }
}
