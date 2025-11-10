using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces.Auth
{
    public interface IRefreshTokenStore
    {
        Task<RefreshTokenIssueResult> CreateAsync(
            string userId,
            DateTimeOffset expiresAtUtc,
            string? tenantId = null,
            string? createdByIp = null,
            string? userAgent = null,
            CancellationToken ct = default);

        Task<RefreshTokenValidationResult> ValidateAsync(
            string refreshToken,
            CancellationToken ct = default);

        Task<RefreshTokenIssueResult> RotateAsync(
            string oldRefreshToken,
            DateTimeOffset newExpiresAtUtc,
            string? createdByIp = null,
            string? userAgent = null,
            CancellationToken ct = default);

        Task RevokeAsync(
            string refreshToken,
            CancellationToken ct = default);
    }

    public sealed record RefreshTokenIssueResult(
        string RefreshToken,
        DateTimeOffset ExpiresAtUtc
    );

    public sealed record RefreshTokenValidationResult(
        bool IsValid,
        string? UserId,
        string? TenantId,
        string? TokenId,
        DateTimeOffset? ExpiresAtUtc,
        bool IsRevoked
    );
}
