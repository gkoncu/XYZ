using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;

namespace XYZ.Application.Features.Coaches.Queries.GetCoachById
{
    public class GetCoachByIdQuery : IRequest<CoachDetailDto>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Coaches.Read;
        public PermissionScope? MinimumScope => PermissionScope.Self;

        public int CoachId { get; set; }
    }
}
