using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach;

public sealed class CreateCoachCommandHandler(
    IApplicationDbContext context,
    IDataScopeService dataScope,
    ICurrentUserService currentUser) : IRequestHandler<CreateCoachCommand, int>
{
    public async Task<int> Handle(CreateCoachCommand request, CancellationToken ct)
    {
        if (currentUser.Role is not ("Admin" or "SuperAdmin"))
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, ct);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var effectiveTenantId = currentUser.TenantId != 0 ? currentUser.TenantId : user.TenantId;

        if (effectiveTenantId <= 0)
            throw new UnauthorizedAccessException("Kulüp bilgisi bulunamadı.");

        if (currentUser.Role == "Admin" && user.TenantId != effectiveTenantId)
            throw new UnauthorizedAccessException("Bu kullanıcı bu kulübe ait değil.");

        var branchOk = await context.Branches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BranchId
                           && b.TenantId == effectiveTenantId
                           && b.IsActive, ct);

        if (!branchOk)
            throw new InvalidOperationException("Branş bulunamadı veya bu kulübe ait değil.");

        var identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
            ? null
            : request.IdentityNumber.Trim();

        if (!string.IsNullOrWhiteSpace(identityNumber))
        {
            var identityExists = await dataScope.Coaches()
                .AsNoTracking()
                .AnyAsync(c => c.IdentityNumber == identityNumber, ct);

            if (identityExists)
                throw new InvalidOperationException("Bu T.C. Kimlik No başka bir koçta kullanılıyor.");
        }

        var coachExistsForUser = await context.Coaches
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.UserId
                           && c.TenantId == effectiveTenantId
                           && c.IsActive, ct);

        if (coachExistsForUser)
            throw new InvalidOperationException("Bu kullanıcı için zaten aktif bir koç kaydı var.");

        var coach = new Coach
        {
            TenantId = effectiveTenantId ?? throw new UnauthorizedAccessException("Kulüp bilgisi bulunamadı."),
            UserId = request.UserId,
            BranchId = request.BranchId,
            IdentityNumber = identityNumber,
            LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Coaches.Add(coach);
        await context.SaveChangesAsync(ct);

        return coach.Id;
    }
}
