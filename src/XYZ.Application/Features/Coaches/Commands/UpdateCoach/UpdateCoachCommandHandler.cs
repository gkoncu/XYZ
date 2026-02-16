using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.UpdateCoach;

public sealed class UpdateCoachCommandHandler(
    IApplicationDbContext context,
    IDataScopeService dataScope,
    ICurrentUserService currentUser) : IRequestHandler<UpdateCoachCommand, int>
{
    public async Task<int> Handle(UpdateCoachCommand request, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

        var coach = await dataScope.Coaches()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CoachId, ct);

        if (coach is null)
            throw new NotFoundException(nameof(Domain.Entities.Coach), request.CoachId);

        var branchOk = await dataScope.Branches()
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BranchId && b.IsActive, ct);

        if (!branchOk)
            throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");

        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email zorunludur.");

        var normalizedEmail = email.ToUpperInvariant();

        var emailInUse = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.TenantId == tenantId
                           && u.NormalizedEmail == normalizedEmail
                           && u.Id != coach.UserId
                           && u.IsActive, ct);

        if (emailInUse)
            throw new InvalidOperationException("Bu e-posta adresi bu kulüp içinde başka bir kullanıcı tarafından kullanılıyor.");

        string? identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : request.IdentityNumber.Trim();
        string? licenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim();

        if (!string.IsNullOrWhiteSpace(identityNumber))
        {
            var identityInUse = await context.Coaches
                .AsNoTracking()
                .AnyAsync(c => c.IdentityNumber == identityNumber && c.Id != coach.Id, ct);

            if (identityInUse)
                throw new InvalidOperationException("Bu T.C. Kimlik No başka bir koçta kullanılıyor.");
        }

        coach.User.FirstName = (request.FirstName ?? string.Empty).Trim();
        coach.User.LastName = (request.LastName ?? string.Empty).Trim();

        coach.User.Email = email;
        coach.User.UserName = email;
        coach.User.NormalizedEmail = normalizedEmail;
        coach.User.NormalizedUserName = normalizedEmail;

        coach.User.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        coach.User.BirthDate = request.BirthDate;

        if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
            throw new InvalidOperationException("Gender değeri geçersiz.");

        if (!Enum.TryParse<BloodType>(request.BloodType, true, out var bloodType))
            throw new InvalidOperationException("BloodType değeri geçersiz.");

        coach.User.Gender = gender;
        coach.User.BloodType = bloodType;

        coach.BranchId = request.BranchId;
        coach.IdentityNumber = identityNumber;
        coach.LicenseNumber = licenseNumber;
        coach.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return coach.Id;
    }
}
