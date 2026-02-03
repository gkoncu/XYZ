using MediatR;

namespace XYZ.Application.Features.Profile.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordCommand : IRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
