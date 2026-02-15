using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Profile.Commands.SwitchMyTenant;

public sealed class SwitchMyTenantCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService current,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<SwitchMyTenantCommand>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ICurrentUserService _current = current;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task Handle(SwitchMyTenantCommand request, CancellationToken ct)
    {
        if (request.TenantId <= 0)
            throw new InvalidOperationException("INVALID_TENANT_ID");

        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

        var exists = await _context.Tenants
            .AsNoTracking()
            .AnyAsync(t => t.Id == request.TenantId, ct);

        if (!exists)
            throw new InvalidOperationException("TENANT_NOT_FOUND");

        var user = await _userManager.FindByIdAsync(_current.UserId);
        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        user.TenantId = request.TenantId;
        user.UpdatedAt = DateTime.UtcNow;

        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
            throw new InvalidOperationException("TENANT_SWITCH_FAILED");
    }
}
