using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using XYZ.API.Services.Auth;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Auth.Login.Commands;
using XYZ.Application.Features.Auth.Logout.Commands;
using XYZ.Application.Features.Auth.Refresh.Commands;
using XYZ.Application.Features.Email.Options;
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
        private readonly IEmailSender _emailSender;
        private readonly IOptions<EmailOptions> _emailOptions;
        private readonly IPasswordSetupLinkBuilder _linkBuilder;

        public AuthController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            IConfiguration config,
            IEmailSender emailSender,
            IOptions<EmailOptions> emailOptions,
            IPasswordSetupLinkBuilder linkBuilder)
        {
            _mediator = mediator;
            _userManager = userManager;
            _env = env;
            _config = config;
            _emailSender = emailSender;
            _emailOptions = emailOptions;
            _linkBuilder = linkBuilder;
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
            if (!IsOriginAllowed(Request.Headers.Origin.ToString()))
            {
                return Forbid();
            }

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

        private bool IsOriginAllowed(string? origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return true;

            // appsettings: Security:AllowedOrigins = "https://app.xyz.com;https://admin.xyz.com"
            var raw = _config["Security:AllowedOrigins"];
            if (string.IsNullOrWhiteSpace(raw))
                return true;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                return false;

            var allowed = raw
                .Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var a in allowed)
            {
                if (!Uri.TryCreate(a, UriKind.Absolute, out var allowedUri))
                    continue;

                if (string.Equals(originUri.Scheme, allowedUri.Scheme, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(originUri.Host, allowedUri.Host, StringComparison.OrdinalIgnoreCase)
                    && originUri.Port == allowedUri.Port)
                {
                    return true;
                }
            }

            return false;
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
            const string generic = "Eğer hesap mevcutsa şifre belirleme bağlantısı gönderilecektir.";

            if (string.IsNullOrWhiteSpace(request.Email))
                return Ok(new { message = generic });

            var email = request.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null || !user.IsActive)
                return Ok(new { message = generic });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var setupUrl = _linkBuilder.Build(user.Id, token);

            if (_emailOptions.Value.Enabled && !string.IsNullOrWhiteSpace(setupUrl))
            {
                var subject = "XYZ - Şifre Belirleme";
                var body = $@"
                    <p>Merhaba,</p>
                    <p>Şifrenizi belirlemek/sıfırlamak için aşağıdaki bağlantıyı kullanın:</p>
                    <p><a href=""{setupUrl}"">{setupUrl}</a></p>
                    <p>Eğer bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>";
                await _emailSender.SendAsync(user.Email!, subject, body, ct);
            }


            if (_env.IsDevelopment())
            {
                PasswordSetupHeaders.Write(Response, user.Id, token, setupUrl);
            }

            return Ok(new { message = generic });
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
