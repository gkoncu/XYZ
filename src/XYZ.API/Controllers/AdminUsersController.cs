using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Auth.Register.Commands;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Produces(MediaTypeNames.Application.Json)]
    public sealed class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public AdminUsersController(
            IMediator mediator,
            ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserRequest request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.TenantId.HasValue)
            {
                return Forbid();
            }

            var callerRole = _currentUser.Role ?? string.Empty;

            if (!callerRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
                request.Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            if (!IsAllowedRole(request.Role))
            {
                return BadRequest("Geçersiz role değeri.");
            }

            var command = new RegisterUserCommand
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                TenantId = _currentUser.TenantId!.Value
            };

            var result = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, result);
        }

        private static bool IsAllowedRole(string role)
        {
            return role.Equals("Student", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Coach", StringComparison.OrdinalIgnoreCase)
                || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

        public sealed record RegisterUserRequest(
            string Email,
            string FirstName,
            string LastName,
            string? PhoneNumber,
            string Role);
    }
}
