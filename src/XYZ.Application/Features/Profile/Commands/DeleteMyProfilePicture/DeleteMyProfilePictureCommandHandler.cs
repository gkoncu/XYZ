using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Profile.Commands.DeleteMyProfilePicture;

public sealed class DeleteMyProfilePictureCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService current,
    IFileService fileService)
    : IRequestHandler<DeleteMyProfilePictureCommand>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _current = current;
    private readonly IFileService _fileService = fileService;

    public async Task Handle(DeleteMyProfilePictureCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == _current.UserId, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
            user.ProfilePictureUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _fileService.DeleteFileAsync(user.ProfilePictureUrl);
            }
            catch
            {
            }
        }

        user.ProfilePictureUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}
