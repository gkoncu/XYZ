using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Profile.Commands.ChangeMyPassword;
using XYZ.Application.Features.Profile.Commands.UpdateMyProfile;
using XYZ.Application.Features.Profile.Queries.GetMyProfile;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
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
