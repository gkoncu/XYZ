using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Data;
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
        private readonly ApplicationDbContext _dbContext;

        public AdminsController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            _mediator = mediator;
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public sealed record UpdateAdminRequest(
            string FirstName,
            string LastName,
            string Email,
            string? PhoneNumber,
            string? IdentityNumber,
            bool CanManageUsers,
            bool CanManageFinance,
            bool CanManageSettings);

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateAdminRequest request,
            CancellationToken cancellationToken)
        {
            int? currentTenantId = null;
            var tenantClaim = User.FindFirst("TenantId")?.Value;
            if (!string.IsNullOrWhiteSpace(tenantClaim) && int.TryParse(tenantClaim, out var parsedTenantId))
            {
                currentTenantId = parsedTenantId;
            }

            var targetTenantId = request.TenantId ?? currentTenantId;

            if (targetTenantId is null)
            {
                return BadRequest("Tenant bilgisi bulunamadı. Lütfen TenantId gönderin veya geçerli bir tenant ile giriş yapın.");
            }

            var email = request.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email zorunludur.");
            }

            var firstName = (request.FirstName ?? string.Empty).Trim();
            var lastName = (request.LastName ?? string.Empty).Trim();
            var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        PhoneNumber = phone,
                        FirstName = firstName,
                        LastName = lastName,
                        TenantId = targetTenantId.Value,
                        IsActive = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        await tx.RollbackAsync(cancellationToken);
                        return BadRequest($"Kullanıcı oluşturulamadı: {errors}");
                    }
                }
                else
                {
                    if (user.TenantId == 0)
                    {
                        user.TenantId = targetTenantId.Value;

                        var updateResult = await _userManager.UpdateAsync(user);
                        if (!updateResult.Succeeded)
                        {
                            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                            await tx.RollbackAsync(cancellationToken);
                            return BadRequest($"Kullanıcı tenant bilgisi güncellenemedi: {errors}");
                        }
                    }
                    else if (user.TenantId != targetTenantId.Value)
                    {
                        await tx.RollbackAsync(cancellationToken);
                        return BadRequest("Bu e-posta adresi farklı bir tenant'a bağlı. Bu kullanıcı için bu kulüpte admin oluşturulamaz.");
                    }
                }

                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    var roleCreate = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!roleCreate.Succeeded)
                    {
                        var errors = string.Join("; ", roleCreate.Errors.Select(e => e.Description));
                        await tx.RollbackAsync(cancellationToken);
                        return BadRequest($"Admin rolü oluşturulamadı: {errors}");
                    }
                }

                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        await tx.RollbackAsync(cancellationToken);
                        return BadRequest($"Kullanıcı Admin rolüne eklenemedi: {errors}");
                    }
                }

                var command = new CreateAdminCommand
                {
                    UserId = user.Id,
                    TenantId = targetTenantId,
                    IdentityNumber = request.IdentityNumber,
                    CanManageUsers = request.CanManageUsers,
                    CanManageFinance = request.CanManageFinance,
                    CanManageSettings = request.CanManageSettings
                };

                var id = await _mediator.Send(command, cancellationToken);

                await tx.CommitAsync(cancellationToken);

                try
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        var hasPassword = await _userManager.HasPasswordAsync(user);
                        if (!hasPassword)
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            Response.Headers["X-Password-UserId"] = user.Id;
                            Response.Headers["X-Password-Token"] = token;
                        }
                    }
                }
                catch
                {

                }

                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (UnauthorizedAccessException)
            {
                await tx.RollbackAsync(cancellationToken);
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
        }

        public sealed record CreateAdminRequest(
            string FirstName,
            string LastName,
            string Email,
            string? PhoneNumber,
            int? TenantId,
            string? IdentityNumber,
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
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var deletedId = await _mediator.Send(
                    new DeleteAdminCommand { AdminId = id },
                    cancellationToken);

                await tx.CommitAsync(cancellationToken);
                return Ok(deletedId);
            }
            catch (UnauthorizedAccessException)
            {
                await tx.RollbackAsync(cancellationToken);
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                await tx.RollbackAsync(cancellationToken);
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                return BadRequest(ex.Message);
            }
        }
    }
}
