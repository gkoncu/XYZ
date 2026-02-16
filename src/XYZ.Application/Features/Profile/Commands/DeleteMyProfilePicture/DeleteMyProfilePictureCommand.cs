using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.DeleteMyProfilePicture;

public sealed class DeleteMyProfilePictureCommand : IRequest, IRequirePermission
{
    public string PermissionKey => PermissionNames.Profiles.UpdateSelf;
    public PermissionScope? MinimumScope => PermissionScope.Self;
}
