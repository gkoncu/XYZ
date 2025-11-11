using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Common.Interfaces.Auth
{
    public interface IUserLookup
    {
        Task<UserIdentity?> FindByEmailAsync(string email, CancellationToken ct = default);
        Task<UserIdentity?> FindByPhoneAsync(string phone, CancellationToken ct = default);
        Task<UserIdentity?> FindByIdentifierAsync(string identifier, CancellationToken ct = default);
    }

    public sealed record UserIdentity(
        string UserId,
        string? Email,
        string? PhoneNumber,
        IReadOnlyCollection<string> Roles,
        string? TenantId,
        string? StudentId,
        string? CoachId,
        string? AdminId
    );
}
