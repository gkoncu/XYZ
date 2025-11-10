using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces
{
    public interface IJwtFactory
    {
        Task<AccessTokenResult> CreateAccessTokenAsync(
            JwtSubject subject,
            CancellationToken ct = default);
    }

    public sealed record JwtSubject(
        string UserId,
        IReadOnlyCollection<string> Roles,
        string? TenantId,
        string? Email,
        string? PhoneNumber,

        string? StudentId,
        string? CoachId,
        string? AdminId,
        IReadOnlyDictionary<string, string>? ExtraClaims
    );

    public sealed record AccessTokenResult(
        string Token,
        DateTimeOffset ExpiresAtUtc
    );
}
