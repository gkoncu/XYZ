using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetTenantsById
{
    public class GetTenantByIdQuery : IRequest<TenantDetailDto>
    {
        public int TenantId { get; set; }
    }
}
