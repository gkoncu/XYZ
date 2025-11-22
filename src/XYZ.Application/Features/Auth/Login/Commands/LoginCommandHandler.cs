using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Auth.Options;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Auth.Login.Commands
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
    {
        private readonly IUserLookup _lookup;
        private readonly IPasswordSignIn _passwordSignIn;
        private readonly IJwtFactory _jwtFactory;
        private readonly IRefreshTokenStore _rtStore;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginCommandHandler(
            IUserLookup lookup,
            IPasswordSignIn passwordSignIn,
            IJwtFactory jwtFactory,
            IRefreshTokenStore rtStore,
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager)
        {
            _lookup = lookup;
            _passwordSignIn = passwordSignIn;
            _jwtFactory = jwtFactory;
            _rtStore = rtStore;
            _jwtOptions = jwtOptions;
            _userManager = userManager;
        }

        public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken ct)
        {
            var userIdentity = await _lookup.FindByIdentifierAsync(request.Identifier, ct);
            if (userIdentity is null)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var pwd = await _passwordSignIn.CheckAsync(userIdentity.UserId, request.Password, ct);
            if (!pwd.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var user = await _userManager.FindByIdAsync(userIdentity.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found.");

            var subject = new JwtSubject(
                UserId: userIdentity.UserId,
                Roles: userIdentity.Roles,
                TenantId: userIdentity.TenantId,
                Email: userIdentity.Email,
                PhoneNumber: userIdentity.PhoneNumber,
                StudentId: null,
                CoachId: null,
                AdminId: null,
                ExtraClaims: null
            );

            var at = await _jwtFactory.CreateAccessTokenAsync(subject, ct);

            var now = DateTimeOffset.UtcNow;
            var rtExpires = now.AddDays(_jwtOptions.Value.RefreshTokenDays);

            var rt = await _rtStore.CreateAsync(
                userId: userIdentity.UserId,
                expiresAtUtc: rtExpires,
                tenantId: userIdentity.TenantId,
                createdByIp: request.CreatedByIp,
                userAgent: request.UserAgent,
                ct: ct);

            var rolesArray = userIdentity.Roles.ToArray();

            return new LoginResultDto(
                AccessToken: at.Token,
                RefreshToken: rt.RefreshToken,
                ExpiresAtUtc: at.ExpiresAtUtc,
                UserId: userIdentity.UserId,
                Email: userIdentity.Email ?? user.Email ?? string.Empty,
                FullName: user.FullName,
                Roles: rolesArray,
                TenantId: userIdentity.TenantId
            );
        }
    }
}
