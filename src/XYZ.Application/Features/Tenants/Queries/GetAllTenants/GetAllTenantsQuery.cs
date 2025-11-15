using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Tenants.Queries.GetAllTenants
{
    public class GetAllTenantsQuery : IRequest<PaginationResult<TenantListItemDto>>
    {
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }
}
