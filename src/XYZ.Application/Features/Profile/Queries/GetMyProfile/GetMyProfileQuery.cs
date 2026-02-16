using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<MyProfileDto>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Profiles.ReadSelf;
    public PermissionScope? MinimumScope => PermissionScope.Self;
}
