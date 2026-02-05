using MediatR;

namespace XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture;

public sealed class UploadMyProfilePictureCommand : IRequest<string>
{
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public string FileName { get; init; } = string.Empty;
}
