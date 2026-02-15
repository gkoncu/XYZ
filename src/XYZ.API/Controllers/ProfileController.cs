using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using XYZ.Application.Features.Profile.Commands.ChangeMyPassword;
using XYZ.Application.Features.Profile.Commands.DeleteMyProfilePicture;
using XYZ.Application.Features.Profile.Commands.SwitchMyTenant;
using XYZ.Application.Features.Profile.Commands.UpdateMyProfile;
using XYZ.Application.Features.Profile.Commands.UploadMyProfilePicture;
using XYZ.Application.Features.Profile.Queries.GetMyProfile;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController(IMediator mediator) : ControllerBase
{
    private const long MaxProfileImageBytes = 10L * 1024 * 1024;

    private readonly IMediator _mediator = mediator;

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
    [RequestSizeLimit(MaxProfileImageBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxProfileImageBytes)]
    public async Task<ActionResult<string>> UploadMyProfilePicture(
        [FromForm] UploadProfilePictureRequest request,
        CancellationToken ct)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
            return BadRequest("Dosya bulunamadı.");

        if (file.Length > MaxProfileImageBytes)
            return BadRequest($"Dosya boyutu {(MaxProfileImageBytes / (1024 * 1024))} MB'den büyük olamaz.");

        try
        {
            await using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);

            var url = await _mediator.Send(new UploadMyProfilePictureCommand
            {
                Content = ms.ToArray(),
                FileName = file.FileName
            }, ct);

            return Ok(ToAbsoluteUrl(url));
        }
        catch (InvalidOperationException ex) when (
            string.Equals(ex.Message, "EMPTY_FILE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ex.Message, "FILE_TOO_LARGE", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ex.Message, "INVALID_FILE_TYPE", StringComparison.OrdinalIgnoreCase))
        {
            return ex.Message.ToUpperInvariant() switch
            {
                "EMPTY_FILE" => BadRequest("Dosya bulunamadı."),
                "FILE_TOO_LARGE" => BadRequest($"Dosya boyutu {(MaxProfileImageBytes / (1024 * 1024))} MB'den büyük olamaz."),
                "INVALID_FILE_TYPE" => BadRequest("Geçersiz resim dosyası. Lütfen JPG/PNG/WEBP yükleyin."),
                _ => BadRequest("Dosya yüklenemedi.")
            };
        }
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

    [HttpPost("me/tenant")]
    public async Task<IActionResult> SwitchMyTenant([FromBody] SwitchMyTenantCommand command, CancellationToken ct)
    {
        try
        {
            await _mediator.Send(command, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.ToUpperInvariant() switch
            {
                "INVALID_TENANT_ID" => BadRequest("Geçersiz tenantId."),
                "TENANT_NOT_FOUND" => NotFound("Tenant bulunamadı."),
                "TENANT_SWITCH_FAILED" => BadRequest("Tenant değiştirilemedi."),
                _ => BadRequest("Tenant değiştirilemedi.")
            };
        }
    }

    private string ToAbsoluteUrl(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return raw;

        if (!raw.StartsWith("/"))
            raw = "/" + raw;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}{raw}";
    }
}
