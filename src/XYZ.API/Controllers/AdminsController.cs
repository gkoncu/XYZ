using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Admins.Commands.CreateAdmin;
using XYZ.Application.Features.Admins.Commands.DeleteAdmin;
using XYZ.Application.Features.Admins.Commands.UpdateAdmin;
using XYZ.Application.Features.Admins.Queries.GetAdminById;
using XYZ.Application.Features.Admins.Queries.GetAllAdmins;
using XYZ.Domain.Entities;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminsController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _mediator = mediator;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<AdminListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<AdminListItemDto>>> GetAll(
            [FromQuery] GetAllAdminsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AdminDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdminDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAdminByIdQuery { AdminId = id },
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateAdminRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateAdminCommand
            {
                AdminId = id,

                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,

                IdentityNumber = request.IdentityNumber,
                CanManageUsers = request.CanManageUsers,
                CanManageFinance = request.CanManageFinance,
                CanManageSettings = request.CanManageSettings
            };

            try
            {
                var updatedId = await _mediator.Send(command, cancellationToken);
                return Ok(updatedId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        public sealed record UpdateAdminRequest(
            string FirstName,
            string LastName,
            string Email,
            string? PhoneNumber,
            string IdentityNumber,
            bool CanManageUsers,
            bool CanManageFinance,
            bool CanManageSettings);

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create(
    [FromBody] CreateAdminRequest request,
    CancellationToken cancellationToken)
        {
            var emailNormalized = request.Email.Trim().ToLower();
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                var password = "Admin123!";

                var createResult = await _userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return BadRequest($"Kullanıcı oluşturulamadı: {errors}");
                }
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    return BadRequest($"Kullanıcı Admin rolüne eklenemedi: {errors}");
                }
            }

            var command = new CreateAdminCommand
            {
                UserId = user.Id,
                TenantId = request.TenantId,
                IdentityNumber = request.IdentityNumber,
                CanManageUsers = request.CanManageUsers,
                CanManageFinance = request.CanManageFinance,
                CanManageSettings = request.CanManageSettings
            };

            try
            {
                var id = await _mediator.Send(command, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public sealed record CreateAdminRequest(
            string FirstName,
            string LastName,
            string Email,
            string? PhoneNumber,
            int? TenantId,
            string IdentityNumber,
            bool CanManageUsers,
            bool CanManageFinance,
            bool CanManageSettings);

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            try
            {
                var deletedId = await _mediator.Send(
                    new DeleteAdminCommand { AdminId = id },
                    cancellationToken);

                return Ok(deletedId);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

    }
}
