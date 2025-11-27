using MediatR;

namespace XYZ.Application.Features.Admins.Commands.DeleteAdmin
{
    public sealed class DeleteAdminCommand : IRequest<int>
    {
        public int AdminId { get; set; }
    }
}
