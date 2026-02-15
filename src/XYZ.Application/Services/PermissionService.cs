using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;

    private Dictionary<string, PermissionScope>? _cache;

    public PermissionService(IApplicationDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task<bool> HasPermissionAsync(string permissionKey, PermissionScope? minScope = null, CancellationToken ct = default)
    {
        if (!PermissionNames.IsKnown(permissionKey))
            throw new InvalidOperationException($"Bilinmeyen permissionKey: '{permissionKey}'. PermissionNames listesinde yok.");

        var scope = await GetScopeAsync(permissionKey, ct);
        if (!scope.HasValue) return false;

        return !minScope.HasValue || scope.Value >= minScope.Value;
    }

    public async Task<PermissionScope?> GetScopeAsync(string permissionKey, CancellationToken ct = default)
    {
        if (_current.IsAuthenticated != true)
            return null;

        var role = _current.Role;
        if (string.IsNullOrWhiteSpace(role))
            return null;

        if (role == RoleNames.SuperAdmin)
            return PermissionScope.AllTenants;

        if (permissionKey.StartsWith("profile.", StringComparison.Ordinal))
            return PermissionScope.Self;

        var map = await GetMapAsync(ct);

        return map.TryGetValue(permissionKey, out var scope)
            ? scope
            : (PermissionScope?)null;
    }

    private async Task<Dictionary<string, PermissionScope>> GetMapAsync(CancellationToken ct)
    {
        if (_cache is not null)
            return _cache;

        var role = _current.Role!;
        var userId = _current.UserId!;

        var rolePerms = await _context.TenantRolePermissions
            .Where(x => x.RoleName == role && x.IsActive)
            .Select(x => new { x.PermissionKey, x.Scope })
            .ToListAsync(ct);

        var userOverrides = await _context.TenantUserPermissionOverrides
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => new { x.PermissionKey, x.Scope })
            .ToListAsync(ct);

        var dict = new Dictionary<string, PermissionScope>(StringComparer.Ordinal);

        static PermissionScope Max(PermissionScope a, PermissionScope b) => a >= b ? a : b;

        foreach (var p in rolePerms)
        {
            dict[p.PermissionKey] = dict.TryGetValue(p.PermissionKey, out var existing)
                ? Max(existing, p.Scope)
                : p.Scope;
        }

        foreach (var p in userOverrides)
        {
            dict[p.PermissionKey] = dict.TryGetValue(p.PermissionKey, out var existing)
                ? Max(existing, p.Scope)
                : p.Scope;
        }

        _cache = dict;
        return dict;
    }
}
