using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Auth.Login.Commands;
using XYZ.Application.Features.Auth.Logout.Commands;
using XYZ.Application.Features.Auth.Refresh.Commands;
using XYZ.Domain.Entities;

namespace XYZ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AuthController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _mediator = mediator;
            _userManager = userManager;
            _env = env;
            _config = config;
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

        [HttpPost("password/forgot")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Ok(new { message = "Eğer hesap mevcutsa şifre belirleme bağlantısı gönderilecektir." });

            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !user.IsActive)
                return Ok(new { message = "Eğer hesap mevcutsa şifre belirleme bağlantısı gönderilecektir." });

            if (_env.IsDevelopment())
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                WritePasswordSetupHeaders(userId: user.Id, token: token);
            }

            return Ok(new { message = "Eğer hesap mevcutsa şifre belirleme bağlantısı gönderilecektir." });
        }

        [HttpPost("password/set")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.UserId)
                || string.IsNullOrWhiteSpace(request.Token)
                || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { error = "UserId, Token ve NewPassword zorunludur." });
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null || !user.IsActive)
                return BadRequest(new { error = "Geçersiz kullanıcı veya token." });

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new { error = errors });
            }

            return Ok(new { message = "Şifre başarıyla güncellendi." });
        }

        private void WritePasswordSetupHeaders(string userId, string token)
        {
            Response.Headers["X-Password-UserId"] = userId;
            Response.Headers["X-Password-Token"] = token;

            var webBaseUrl = _config["Web:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(webBaseUrl))
            {
                var url = $"{webBaseUrl.TrimEnd('/')}/Account/SetPassword?uid={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
                Response.Headers["X-Password-Setup-Url"] = url;
            }
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

        public sealed record ForgotPasswordRequest(string Email);
        public sealed record SetPasswordRequest(string UserId, string Token, string NewPassword);
    }
}
