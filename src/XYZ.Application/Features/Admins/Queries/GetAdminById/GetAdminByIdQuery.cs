using MediatR;

namespace XYZ.Application.Features.Admins.Queries.GetAdminById
{
    public sealed class GetAdminByIdQuery : IRequest<AdminDetailDto?>
    {
        public int AdminId { get; set; }
    }
}
