using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public RefreshCommandHandler(
            IRefreshTokenStore rtStore,
            IJwtFactory jwtFactory,
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager)
        {
            _rtStore = rtStore;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions;
            _userManager = userManager;
        }

        public async Task<LoginResultDto> Handle(RefreshCommand request, CancellationToken ct)
        {
            var valid = await _rtStore.ValidateAsync(request.RefreshToken, ct);
            if (!valid.IsValid || string.IsNullOrWhiteSpace(valid.UserId))
                throw new UnauthorizedAccessException("Invalid refresh token.");

            var user = await _userManager.FindByIdAsync(valid.UserId);
            if (user is null)
                throw new UnauthorizedAccessException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            var subject = new JwtSubject(
                UserId: user.Id,
                Roles: roles,
                TenantId: user.TenantId.ToString(),
                Email: user.Email,
                PhoneNumber: user.PhoneNumber,
                StudentId: null,
                CoachId: null,
                AdminId: null,
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
                ExpiresAtUtc: at.ExpiresAtUtc
            );
        }
    }
}
