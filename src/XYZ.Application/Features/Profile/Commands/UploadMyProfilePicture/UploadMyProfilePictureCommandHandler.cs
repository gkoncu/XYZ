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

        const int maxBytes = 2 * 1024 * 1024;
        if (request.Content.Length > maxBytes)
            throw new InvalidOperationException("FILE_TOO_LARGE");

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == _current.UserId, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        var ext = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("INVALID_FILE_TYPE");

        var safeName = $"profile_{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
            user.ProfilePictureUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            await _fileService.DeleteFileAsync(user.ProfilePictureUrl);
        }

        await using var ms = new MemoryStream(request.Content);
        var url = await _fileService.UploadFileAsync(ms, safeName);

        user.ProfilePictureUrl = url;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return url;
    }
}
