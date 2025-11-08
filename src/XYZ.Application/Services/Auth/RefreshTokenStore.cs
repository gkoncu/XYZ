using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Interfaces.Auth;
using XYZ.Application.Data;

namespace XYZ.Application.Services.Auth
{
    public sealed class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly ApplicationDbContext _db;

        public RefreshTokenStore(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RefreshTokenIssueResult> CreateAsync(
            string userId,
            DateTimeOffset expiresAtUtc,
            string? tenantId = null,
            string? createdByIp = null,
            string? userAgent = null,
            CancellationToken ct = default)
        {
            var raw = GenerateToken();
            var hash = HashToken(raw);

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TenantId = tenantId,
                Hash = hash,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc = expiresAtUtc,
                CreatedByIp = createdByIp,
                UserAgent = userAgent
            };

            _db.Set<RefreshToken>().Add(entity);
            await _db.SaveChangesAsync(ct);

            return new RefreshTokenIssueResult(raw, expiresAtUtc);
        }

        public async Task<RefreshTokenValidationResult> ValidateAsync(string refreshToken, CancellationToken ct = default)
        {
            var hash = HashToken(refreshToken);

            var rt = await _db.Set<RefreshToken>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Hash == hash, ct);

            if (rt is null)
            {
                return new RefreshTokenValidationResult(
                    IsValid: false, UserId: null, TenantId: null, TokenId: null, ExpiresAtUtc: null, IsRevoked: false);
            }

            var now = DateTimeOffset.UtcNow;
            var expired = now >= rt.ExpiresAtUtc;
            var revoked = rt.RevokedAtUtc.HasValue;

            var valid = !expired && !revoked;

            return new RefreshTokenValidationResult(
                IsValid: valid,
                UserId: rt.UserId,
                TenantId: rt.TenantId,
                TokenId: rt.Id.ToString(),
                ExpiresAtUtc: rt.ExpiresAtUtc,
                IsRevoked: revoked
            );
        }

        public async Task<RefreshTokenIssueResult> RotateAsync(
            string oldRefreshToken,
            DateTimeOffset newExpiresAtUtc,
            string? createdByIp = null,
            string? userAgent = null,
            CancellationToken ct = default)
        {
            var oldHash = HashToken(oldRefreshToken);

            var existing = await _db.Set<RefreshToken>()
                .FirstOrDefaultAsync(x => x.Hash == oldHash, ct);

            if (existing is null)
                throw new InvalidOperationException("Refresh token not found.");

            if (existing.RevokedAtUtc.HasValue || existing.ExpiresAtUtc <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Refresh token is not valid for rotation.");

            // Revoke old
            existing.RevokedAtUtc = DateTimeOffset.UtcNow;

            // Create new
            var raw = GenerateToken();
            var hash = HashToken(raw);

            var replacement = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = existing.UserId,
                TenantId = existing.TenantId,
                Hash = hash,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ExpiresAtUtc = newExpiresAtUtc,
                CreatedByIp = createdByIp,
                UserAgent = userAgent,
                ReplacedByTokenId = existing.Id
            };

            _db.Set<RefreshToken>().Add(replacement);
            await _db.SaveChangesAsync(ct);

            return new RefreshTokenIssueResult(raw, newExpiresAtUtc);
        }

        public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
        {
            var hash = HashToken(refreshToken);

            var existing = await _db.Set<RefreshToken>()
                .FirstOrDefaultAsync(x => x.Hash == hash, ct);

            if (existing is null) return;

            if (!existing.RevokedAtUtc.HasValue)
            {
                existing.RevokedAtUtc = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        // === helpers ===

        private static string GenerateToken()
        {
            // 32 byte crypto-random → URL-safe Base64 (padding'siz).
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            var b64 = Convert.ToBase64String(bytes);
            return ToUrlSafe(b64);
        }

        private static string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            var b64 = Convert.ToBase64String(hash);
            return ToUrlSafe(b64);
        }

        private static string ToUrlSafe(string base64)
            => base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
