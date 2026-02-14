using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById
{
    public class GetClassSessionByIdQuery : IRequest<ClassSessionDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.ClassSessions.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int Id { get; set; }
    }
}
