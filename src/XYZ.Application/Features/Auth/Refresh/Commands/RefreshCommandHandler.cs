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

namespace XYZ.Application.Features.Auth.Refresh.Commands
{
    public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, LoginResultDto>
    {
        private readonly IRefreshTokenStore _rtStore;
        private readonly IJwtFactory _jwtFactory;
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserLookup _userLookup;

        public RefreshCommandHandler(
            IRefreshTokenStore rtStore,
            IJwtFactory jwtFactory,
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager,
            IUserLookup userLookup)
        {
            _rtStore = rtStore;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions;
            _userManager = userManager;
            _userLookup = userLookup;
        }

        public async Task<LoginResultDto> Handle(RefreshCommand request, CancellationToken ct)
        {
            var valid = await _rtStore.ValidateAsync(request.RefreshToken, ct);
            if (!valid.IsValid || string.IsNullOrWhiteSpace(valid.UserId))
                throw new UnauthorizedAccessException("Invalid refresh token.");

            var user = await _userManager.FindByIdAsync(valid.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found.");

            var identity = await _userLookup.FindByUserIdAsync(user.Id, ct);
            if (identity is null)
                throw new UnauthorizedAccessException("User identity not found.");

            var rolesArray = identity.Roles.ToArray();

            var subject = new JwtSubject(
                UserId: identity.UserId,
                Roles: rolesArray,
                TenantId: identity.TenantId,
                Email: identity.Email ?? user.Email,
                PhoneNumber: identity.PhoneNumber ?? user.PhoneNumber,

                StudentId: identity.StudentId,
                CoachId: identity.CoachId,
                AdminId: identity.AdminId,

                ExtraClaims: null
            );

            var at = await _jwtFactory.CreateAccessTokenAsync(subject, ct);

            var now = DateTimeOffset.UtcNow;
            var newRtExpires = now.AddDays(_jwtOptions.Value.RefreshTokenDays);

            var rotated = await _rtStore.RotateAsync(
                oldRefreshToken: request.RefreshToken,
                newExpiresAtUtc: newRtExpires,
                createdByIp: request.CreatedByIp,
                userAgent: request.UserAgent,
                ct: ct);

            return new LoginResultDto(
                AccessToken: at.Token,
                RefreshToken: rotated.RefreshToken,
                ExpiresAtUtc: at.ExpiresAtUtc,
                UserId: user.Id,
                Email: user.Email ?? string.Empty,
                FullName: user.FullName,
                Roles: rolesArray,
                TenantId: identity.TenantId
            );
        }
    }
}
