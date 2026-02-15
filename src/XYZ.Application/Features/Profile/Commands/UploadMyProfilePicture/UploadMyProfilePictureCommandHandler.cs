using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture;

public sealed class UploadMyProfilePictureCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService current,
    IFileService fileService)
    : IRequestHandler<UploadMyProfilePictureCommand, string>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _current = current;
    private readonly IFileService _fileService = fileService;

    public async Task<string> Handle(UploadMyProfilePictureCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

        if (request.Content is null || request.Content.Length == 0)
            throw new InvalidOperationException("EMPTY_FILE");

        const int maxBytes = 10 * 1024 * 1024;
        if (request.Content.Length > maxBytes)
            throw new InvalidOperationException("FILE_TOO_LARGE");

        var detected = DetectImageType(request.Content);
        if (detected is null)
            throw new InvalidOperationException("INVALID_FILE_TYPE");

        var ext = detected switch
        {
            ImageType.Jpeg => ".jpg",
            ImageType.Png => ".png",
            ImageType.Webp => ".webp",
            _ => ".jpg"
        };

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == _current.UserId, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        var safeName = $"profile_{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
            user.ProfilePictureUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            try { await _fileService.DeleteFileAsync(user.ProfilePictureUrl); }
            catch { }
        }

        await using var ms = new MemoryStream(request.Content);
        var url = await _fileService.UploadFileAsync(ms, safeName);

        user.ProfilePictureUrl = url;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return url;
    }

    private enum ImageType { Jpeg, Png, Webp }

    private static ImageType? DetectImageType(byte[] bytes)
    {
        // JPEG: FF D8 FF
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return ImageType.Jpeg;

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
            bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
            return ImageType.Png;

        // WEBP: "RIFF" .... "WEBP"
        if (bytes.Length >= 12 &&
            bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
            bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
            return ImageType.Webp;

        return null;
    }
}
