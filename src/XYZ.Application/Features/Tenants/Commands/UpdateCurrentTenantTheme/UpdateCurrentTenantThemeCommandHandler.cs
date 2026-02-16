using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;

public sealed class UpdateCurrentTenantThemeCommandHandler : IRequestHandler<UpdateCurrentTenantThemeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _current;

    public UpdateCurrentTenantThemeCommandHandler(IApplicationDbContext context, ICurrentUserService current)
    {
        _context = context;
        _current = current;
    }

    public async Task Handle(UpdateCurrentTenantThemeCommand request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || !_current.TenantId.HasValue)
            throw new UnauthorizedAccessException("Tenant bulunamadı.");

        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == _current.TenantId.Value, ct);
        if (tenant is null)
            throw new InvalidOperationException("Tenant bulunamadı.");

        tenant.PrimaryColor = request.PrimaryColor;
        tenant.SecondaryColor = request.SecondaryColor;
        tenant.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();

        await _context.SaveChangesAsync(ct);
    }
}
