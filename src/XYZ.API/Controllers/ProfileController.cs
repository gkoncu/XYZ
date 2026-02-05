using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Profile.Commands.ChangeMyPassword;
using XYZ.Application.Features.Profile.Commands.DeleteMyProfilePicture;
using XYZ.Application.Features.Profile.Commands.UpdateMyProfile;
using XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture;
using XYZ.Application.Features.Profile.Queries.GetMyProfile;
using XYZ.Domain.Entities;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public ProfileController(IMediator mediator, UserManager<ApplicationUser> userManager, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _userManager = userManager;
        _currentUser = currentUser;
        _env = env;
    }

    [HttpGet("me")]
    public async Task<ActionResult<MyProfileDto>> GetMyProfile(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMyProfileQuery(), ct));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("me/picture")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<ActionResult<string>> UploadMyProfilePicture(
    [FromForm] UploadProfilePictureRequest request,
    CancellationToken ct)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("Dosya bulunamadı.");

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Sadece resim dosyaları yüklenebilir.");

        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"profile_{userId}_{DateTime.UtcNow.Ticks}.webp";
        var filePath = Path.Combine(uploadsRoot, fileName);

        var user = await _userManager.FindByIdAsync(userId);
        if (!string.IsNullOrWhiteSpace(user?.ProfilePictureUrl))
        {
            var oldPath = Path.Combine(
                _env.WebRootPath,
                user.ProfilePictureUrl.TrimStart('/'));

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        await using (var inputStream = file.OpenReadStream())
        using (var image = await Image.LoadAsync(inputStream, ct))
        {
            var encoder = new WebpEncoder
            {
                Quality = 80,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(filePath, encoder, ct);
        }

        user!.ProfilePictureUrl = $"/uploads/{fileName}";
        await _userManager.UpdateAsync(user);

        return Ok(user.ProfilePictureUrl);
    }

    [HttpDelete("me/picture")]
    public async Task<IActionResult> DeleteMyProfilePicture(CancellationToken ct)
    {
        await _mediator.Send(new DeleteMyProfilePictureCommand(), ct);
        return NoContent();
    }

    [HttpPost("me/password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordCommand command, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (string.Equals(ex.Message, "INVALID_CURRENT_PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    errorCode = "invalid_current_password",
                    error = "Invalid current password."
                });
            }

            if (string.Equals(ex.Message, "PASSWORD_POLICY_FAILED", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    errorCode = "password_policy_failed",
                    error = "Password policy validation failed."
                });
            }

            return BadRequest(new
            {
                errorCode = "change_password_failed",
                error = "Change password failed."
            });
        }
    }
}
