using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Auth.Register.Commands
{
    public sealed class RegisterUserCommandHandler
        : IRequestHandler<RegisterUserCommand, RegisterUserResultDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterUserResultDto> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            var normalizedEmail = _userManager.NormalizeEmail(request.Email);

            var emailExistsInTenant = await _userManager.Users
                .Where(u => u.TenantId == request.TenantId)
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

            if (emailExistsInTenant)
            {
                throw new InvalidOperationException(
                    $"Bu e-posta ({request.Email}) bu tenant altında zaten kayıtlı.");
            }

            var tempPassword = _configuration["Auth:DefaultTempPassword"]
                               ?? "SportifyTemp#123";

            var fullName = $"{request.FirstName} {request.LastName}".Trim();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PhoneNumber = request.PhoneNumber,
                TenantId = request.TenantId,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
            };

            var createResult = await _userManager.CreateAsync(user, tempPassword);

            if (!createResult.Succeeded)
            {
                var errorMessage = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogWarning("Kullanıcı oluşturulamadı. Email: {Email}, Hata: {Error}",
                    request.Email, errorMessage);

                throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {errorMessage}");
            }

            var roleExists = await _roleManager.RoleExistsAsync(request.Role);
            if (!roleExists)
            {
                var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(request.Role));
                if (!roleCreateResult.Succeeded)
                {
                    var errorMessage = string.Join("; ", roleCreateResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("Rol oluşturulamadı. Role: {Role}, Hata: {Error}",
                        request.Role, errorMessage);

                    throw new InvalidOperationException($"Rol oluşturulamadı ({request.Role}): {errorMessage}");
                }
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, request.Role);

            if (!addToRoleResult.Succeeded)
            {
                var errorMessage = string.Join("; ", addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                _logger.LogError("Kullanıcıya rol atanamadı. UserId: {UserId}, Role: {Role}, Hata: {Error}",
                    user.Id, request.Role, errorMessage);

                throw new InvalidOperationException($"Kullanıcıya rol atanamadı ({request.Role}): {errorMessage}");
            }

            _logger.LogInformation("Yeni kullanıcı oluşturuldu. UserId: {UserId}, Email: {Email}, Role: {Role}, TenantId: {TenantId}",
                user.Id, user.Email, request.Role, request.TenantId);

            return new RegisterUserResultDto
            {
                UserId = user.Id.ToString()!,
                Email = user.Email ?? request.Email,
                FullName = user.FullName,
                Role = request.Role,
                TenantId = request.TenantId,
                TempPassword = tempPassword
            };
        }
    }
}
