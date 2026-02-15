using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetTenantsById
{
    public class GetTenantByIdQuery : IRequest<TenantDetailDto>, IRequirePermission
    {
        public int TenantId { get; set; }

        public string PermissionKey => PermissionNames.Tenants.Read;
        public PermissionScope? MinimumScope => PermissionScope.AllTenants;
    }
}
