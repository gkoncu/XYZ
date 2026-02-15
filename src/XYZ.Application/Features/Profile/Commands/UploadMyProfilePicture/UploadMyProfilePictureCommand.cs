using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture;

public sealed class UploadMyProfilePictureCommand : IRequest<string>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Profiles.UpdateSelf;
    public PermissionScope? MinimumScope => PermissionScope.Self;

    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = string.Empty;
}
