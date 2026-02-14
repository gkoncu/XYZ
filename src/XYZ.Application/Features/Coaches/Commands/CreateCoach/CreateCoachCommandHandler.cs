using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

        var branch = await dataScope.Branches()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId, ct);

        if (branch is null)
            throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId
                                     && u.IsActive
                                     && u.TenantId == tenantId, ct);

        if (user is null)
            throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
            ? null
            : request.IdentityNumber.Trim();

        if (!string.IsNullOrWhiteSpace(identityNumber))
        {
            var identityExists = await context.Coaches
                .AsNoTracking()
                .AnyAsync(c => c.IdentityNumber == identityNumber, ct);

            if (identityExists)
                throw new InvalidOperationException("Bu T.C. Kimlik No başka bir koçta kullanılıyor.");
        }

        var coachExistsForUser = await context.Coaches
            .AsNoTracking()
            .AnyAsync(c => c.UserId == request.UserId, ct);

        if (coachExistsForUser)
            throw new InvalidOperationException("Bu kullanıcı için zaten aktif bir koç kaydı var.");

        var nowUtc = DateTime.UtcNow;

        var coach = new Coach
        {
            TenantId = tenantId,
            UserId = request.UserId,
            BranchId = request.BranchId,
            IdentityNumber = identityNumber,
            LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim(),
            IsActive = true,
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        context.Coaches.Add(coach);
        await context.SaveChangesAsync(ct);

        return coach.Id;
    }
}
