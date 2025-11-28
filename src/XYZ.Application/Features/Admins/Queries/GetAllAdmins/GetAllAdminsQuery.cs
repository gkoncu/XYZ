using MediatR;
using XYZ.Application.Common.Models;

namespace XYZ.Application.Features.Admins.Queries.GetAllAdmins
{
    public sealed class GetAllAdminsQuery : IRequest<PaginationResult<AdminListItemDto>>
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
