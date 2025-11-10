using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Auth.Login.Commands;
using XYZ.Application.Features.Auth.Logout.Commands;
using XYZ.Application.Features.Auth.Refresh.Commands;

namespace XYZ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            try
            {
                var cmd = new LoginCommand(
                    Identifier: request.Identifier,
                    Password: request.Password,
                    CreatedByIp: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent: Request.Headers.UserAgent.ToString());

                var result = await _mediator.Send(cmd, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid credentials." });
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(LoginResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        {
            try
            {
                var cmd = new RefreshCommand(
                    RefreshToken: request.RefreshToken,
                    CreatedByIp: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent: Request.Headers.UserAgent.ToString());

                var result = await _mediator.Send(cmd, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid refresh token." });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
        {
            var ok = await _mediator.Send(new LogoutCommand(request.RefreshToken), ct);
            return ok ? Ok() : BadRequest();
        }


        [HttpGet]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            return Ok(new { message = "You have accessed a protected endpoint." });
        }

        // === Request models ===
        public sealed record LoginRequest(string Identifier, string Password);
        public sealed record RefreshRequest(string RefreshToken);
        public sealed record LogoutRequest(string RefreshToken);
    }
}

