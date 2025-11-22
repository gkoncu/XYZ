using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme
{
    public class GetCurrentTenantThemeQuery : IRequest<TenantThemeDto>
    {
    }
}
