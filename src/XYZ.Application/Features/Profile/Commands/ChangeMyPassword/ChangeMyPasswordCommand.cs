using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Profile.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommand : IRequest, IRequirePermission
{
    public string PermissionKey => PermissionNames.Profiles.ChangePasswordSelf;
    public PermissionScope? MinimumScope => PermissionScope.Self;

    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
