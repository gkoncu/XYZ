using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.API.Services.Auth;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Application.Data;
using XYZ.Application.Features.Email.Options;
using XYZ.Application.Features.Students.Commands.ActivateStudent;
using XYZ.Application.Features.Students.Commands.CreateStudent;
using XYZ.Application.Features.Students.Commands.DeactivateStudent;
using XYZ.Application.Features.Students.Commands.DeleteStudent;
using XYZ.Application.Features.Students.Commands.UpdateStudent;
using XYZ.Application.Features.Students.Queries.GetAllStudents;
using XYZ.Application.Features.Students.Queries.GetStudentById;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly IPermissionService _permissions;

    private readonly IEmailSender _emailSender;
    private readonly IOptions<EmailOptions> _emailOptions;
    private readonly IWebHostEnvironment _env;
    private readonly IPasswordSetupLinkBuilder _linkBuilder;

    public StudentsController(
        IMediator mediator,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db,
        IPermissionService permissions,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        IWebHostEnvironment env,
        IPasswordSetupLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _permissions = permissions;

        _emailSender = emailSender;
        _emailOptions = emailOptions;
        _env = env;
        _linkBuilder = linkBuilder;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationResult<StudentListItemDto>>> GetAll(
        [FromQuery] GetAllStudentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StudentDetailDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentByIdQuery { StudentId = id }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<ActionResult<int>> Create([FromBody] CreateStudentRequestDTO request, CancellationToken ct)
    {
        var canCreate = await _permissions.HasPermissionAsync(PermissionNames.Students.Create, PermissionScope.OwnClasses, ct);
        if (!canCreate)
            return Forbid();

        int? tenantId = null;
        var tenantClaim = User.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantClaim) && int.TryParse(tenantClaim, out var parsedTenantId))
            tenantId = parsedTenantId;

        if (!tenantId.HasValue)
            return BadRequest("Tenant bilgisi bulunamadı. Geçerli bir tenant ile giriş yapın.");

        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email zorunludur.");

        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            return BadRequest("Geçerli bir cinsiyet değeri giriniz.");

        if (!Enum.TryParse<BloodType>(request.BloodType, true, out var bloodType))
            return BadRequest("Geçerli bir kan grubu değeri giriniz.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName?.Trim() ?? string.Empty,
                    LastName = request.LastName?.Trim() ?? string.Empty,
                    TenantId = tenantId.Value,
                    IsActive = true,
                    BirthDate = request.BirthDate,
                    Gender = gender,
                    BloodType = bloodType
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    await tx.RollbackAsync(ct);
                    return BadRequest($"Kullanıcı oluşturulamadı: {errors}");
                }
            }
            else
            {
                if (user.TenantId != 0 && user.TenantId != tenantId.Value)
                {
                    await tx.RollbackAsync(ct);
                    return BadRequest("Bu e-posta adresi farklı bir tenant'a bağlı. Bu kullanıcı için bu kulüpte öğrenci oluşturulamaz.");
                }

                if (user.TenantId == 0)
                {
                    user.TenantId = tenantId.Value;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                        await tx.RollbackAsync(ct);
                        return BadRequest($"Kullanıcı tenant bilgisi güncellenemedi: {errors}");
                    }
                }
            }

            var studentRole = RoleNames.Student;

            if (!await _roleManager.RoleExistsAsync(studentRole))
            {
                var roleCreate = await _roleManager.CreateAsync(new IdentityRole(studentRole));
                if (!roleCreate.Succeeded)
                {
                    var errors = string.Join("; ", roleCreate.Errors.Select(e => e.Description));
                    await tx.RollbackAsync(ct);
                    return BadRequest($"Student rolü oluşturulamadı: {errors}");
                }
            }

            if (!await _userManager.IsInRoleAsync(user, studentRole))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, studentRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    await tx.RollbackAsync(ct);
                    return BadRequest($"Kullanıcı Student rolüne eklenemedi: {errors}");
                }
            }

            var command = new CreateStudentCommand
            {
                UserId = user.Id,
                ClassId = request.ClassId,
                IdentityNumber = request.IdentityNumber,
                Address = request.Address,

                Parent1FirstName = request.Parent1FirstName,
                Parent1LastName = request.Parent1LastName,
                Parent1Email = request.Parent1Email,
                Parent1PhoneNumber = request.Parent1PhoneNumber,

                Parent2FirstName = request.Parent2FirstName,
                Parent2LastName = request.Parent2LastName,
                Parent2Email = request.Parent2Email,
                Parent2PhoneNumber = request.Parent2PhoneNumber,

                MedicalInformation = request.MedicalInformation,
                Notes = request.Notes
            };

            var id = await _mediator.Send(command, ct);

            await tx.CommitAsync(ct);

            try
            {
                var hasPassword = await _userManager.HasPasswordAsync(user);
                if (!hasPassword)
                {
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
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync(ct);
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<int>> Update(int id, [FromBody] UpdateStudentCommand command, CancellationToken cancellationToken)
    {
        command.StudentId = id;
        var updatedId = await _mediator.Send(command, cancellationToken);
        return Ok(updatedId);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<int>> Delete(int id, CancellationToken cancellationToken)
    {
        var deletedId = await _mediator.Send(new DeleteStudentCommand { StudentId = id }, cancellationToken);
        return Ok(deletedId);
    }

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<int>> Activate(int id, CancellationToken cancellationToken)
    {
        var resultId = await _mediator.Send(new ActivateStudentCommand { StudentId = id }, cancellationToken);
        return Ok(resultId);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<int>> Deactivate(int id, CancellationToken cancellationToken)
    {
        var resultId = await _mediator.Send(new DeactivateStudentCommand { StudentId = id }, cancellationToken);
        return Ok(resultId);
    }
}
