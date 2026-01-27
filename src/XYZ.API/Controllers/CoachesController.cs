using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using XYZ.API.Services.Auth;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Data;
using XYZ.Application.Features.Coaches.Commands.CreateCoach;
using XYZ.Application.Features.Coaches.Commands.DeleteCoach;
using XYZ.Application.Features.Coaches.Commands.UpdateCoach;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Coaches.Queries.GetCoachById;
using XYZ.Application.Features.Email.Options;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CoachesController(
    IMediator mediator,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext db,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions,
    IWebHostEnvironment env,
    IPasswordSetupLinkBuilder linkBuilder) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public async Task<IActionResult> GetAll([FromQuery] GetAllCoachesQuery query, CancellationToken ct)
        => Ok(await mediator.Send(query, ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetCoachByIdQuery { CoachId = id }, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<ActionResult<int>> Create([FromBody] CreateCoachRequestDTO request, CancellationToken ct)
    {
        int? currentTenantId = null;
        var tenantClaim = User.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantClaim) && int.TryParse(tenantClaim, out var parsedTenantId))
            currentTenantId = parsedTenantId;

        var targetTenantId = request.TenantId ?? currentTenantId;
        if (targetTenantId is null)
            return BadRequest("Tenant bilgisi bulunamadı. Lütfen TenantId gönderin veya geçerli bir tenant ile giriş yapın.");

        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email zorunludur.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
                    return BadRequest("Gender değeri geçersiz.");

                if (!Enum.TryParse<BloodType>(request.BloodType, true, out var bloodType))
                    return BadRequest("BloodType değeri geçersiz.");

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                    FirstName = (request.FirstName ?? string.Empty).Trim(),
                    LastName = (request.LastName ?? string.Empty).Trim(),
                    TenantId = targetTenantId.Value,
                    BirthDate = request.BirthDate,
                    Gender = gender,
                    BloodType = bloodType,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    await tx.RollbackAsync(ct);
                    return BadRequest($"Kullanıcı oluşturulamadı: {errors}");
                }
            }
            else
            {
                if (user.TenantId != 0 && user.TenantId != targetTenantId.Value)
                {
                    await tx.RollbackAsync(ct);
                    return BadRequest("Bu e-posta adresi farklı bir tenant'a bağlı. Bu kullanıcı için bu kulüpte koç oluşturulamaz.");
                }

                if (user.TenantId == 0)
                {
                    user.TenantId = targetTenantId.Value;
                    var updateResult = await userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                        await tx.RollbackAsync(ct);
                        return BadRequest($"Kullanıcı tenant bilgisi güncellenemedi: {errors}");
                    }
                }
            }

            const string coachRole = "Coach";
            if (!await roleManager.RoleExistsAsync(coachRole))
                await roleManager.CreateAsync(new IdentityRole(coachRole));

            if (!await userManager.IsInRoleAsync(user, coachRole))
            {
                var roleResult = await userManager.AddToRoleAsync(user, coachRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    await tx.RollbackAsync(ct);
                    return BadRequest($"Kullanıcı Coach rolüne eklenemedi: {errors}");
                }
            }

            var command = new CreateCoachCommand
            {
                UserId = user.Id,
                BranchId = request.BranchId,
                IdentityNumber = request.IdentityNumber,
                LicenseNumber = request.LicenseNumber
            };

            var id = await mediator.Send(command, ct);

            await tx.CommitAsync(ct);

            try
            {
                var hasPassword = await userManager.HasPasswordAsync(user);
                if (!hasPassword)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var setupUrl = linkBuilder.Build(user.Id, token);

                    if (emailOptions.Value.Enabled && !string.IsNullOrWhiteSpace(setupUrl))
                    {
                        var subject = "XYZ - Şifre Belirleme";
                        var body = $@"
                        <p>Merhaba,</p>
                        <p>Şifrenizi belirlemek/sıfırlamak için aşağıdaki bağlantıyı kullanın:</p>
                        <p><a href=""{setupUrl}"">{setupUrl}</a></p>
                        <p>Eğer bu isteği siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>";
                        await emailSender.SendAsync(user.Email!, subject, body, ct);
                    }

                    if (env.IsDevelopment())
                    {
                        PasswordSetupHeaders.Write(Response, user.Id, token, setupUrl);
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
            await tx.RollbackAsync(ct);
            return Forbid();
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            await tx.RollbackAsync(ct);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCoachCommand command, CancellationToken ct)
    {
        if (id != command.CoachId) return BadRequest("Id uyuşmuyor.");
        return Ok(await mediator.Send(command, ct));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => Ok(await mediator.Send(new DeleteCoachCommand { CoachId = id }, ct));

    public sealed record CreateCoachRequestDTO(
        string FirstName,
        string LastName,
        string Email,
        string? PhoneNumber,
        DateTime BirthDate,
        string Gender,
        string BloodType,
        int BranchId,
        string? IdentityNumber,
        string? LicenseNumber,
        int? TenantId);
}
