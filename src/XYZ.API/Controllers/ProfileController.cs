using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
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
    private readonly IApplicationDbContext _db;

    public ProfileController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IWebHostEnvironment env,
        IApplicationDbContext db)
    {
        _mediator = mediator;
        _userManager = userManager;
        _currentUser = currentUser;
        _env = env;
        _db = db;
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

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("Dosya boyutu 2 MB'den büyük olamaz.");

        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"profile_{Guid.NewGuid():N}_{DateTime.UtcNow.Ticks}.webp";
        var filePath = Path.Combine(uploadsRoot, fileName);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
        {
            var oldPath = Path.Combine(
                _env.WebRootPath,
                user.ProfilePictureUrl.TrimStart('/'));

            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        try
        {
            await using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream, ct);

            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(512, 512)
                }));

            var encoder = new WebpEncoder
            {
                Quality = 80
            };

            await using var output = System.IO.File.Create(filePath);
            await image.SaveAsync(output, encoder, ct);
        }
        catch
        {
            return BadRequest("Geçersiz resim dosyası. Lütfen farklı bir görsel deneyin.");
        }

        user.ProfilePictureUrl = $"/uploads/{fileName}";
        await _userManager.UpdateAsync(user);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok($"{baseUrl}{user.ProfilePictureUrl}");
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

    // ===== SUPERADMIN: Tenant switch =====
    public sealed class SwitchMyTenantRequest
    {
        public int TenantId { get; set; }
    }

    [HttpPost("me/tenant")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SwitchMyTenant([FromBody] SwitchMyTenantRequest request, CancellationToken ct)
    {
        if (request.TenantId <= 0)
            return BadRequest("Geçersiz tenantId.");

        var exists = await _db.Tenants.AsNoTracking().AnyAsync(t => t.Id == request.TenantId, ct);
        if (!exists)
            return NotFound("Tenant bulunamadı.");

        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        user.TenantId = request.TenantId;

        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
            return BadRequest("Tenant değiştirilemedi.");

        return NoContent();
    }
}
