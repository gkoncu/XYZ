using MediatR;
using Microsoft.EntityFrameworkCore;
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
        if (currentUser.Role is not ("Admin" or "SuperAdmin"))
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");

        var coach = await dataScope.Coaches()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CoachId, ct);

        if (coach is null)
            throw new NotFoundException(nameof(Domain.Entities.Coach), request.CoachId);

        var effectiveTenantId = currentUser.TenantId != 0 ? currentUser.TenantId : coach.TenantId;

        if (effectiveTenantId <= 0)
            throw new UnauthorizedAccessException("Tenant bilgisi bulunamadı.");

        var branchOk = await context.Branches
            .AsNoTracking()
            .AnyAsync(b => b.Id == request.BranchId
                           && b.TenantId == effectiveTenantId
                           && b.IsActive, ct);

        if (!branchOk)
            throw new InvalidOperationException("Branş bulunamadı veya bu tenant'a ait değil.");

        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Email zorunludur.");

        var normalizedEmail = email.ToUpperInvariant();

        var emailInUse = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.TenantId == effectiveTenantId
                           && u.NormalizedEmail == normalizedEmail
                           && u.Id != coach.UserId
                           && u.IsActive, ct);

        if (emailInUse)
            throw new InvalidOperationException("Bu e-posta adresi bu tenant içinde başka bir kullanıcı tarafından kullanılıyor.");

        string? identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : request.IdentityNumber.Trim();
        string? licenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? null : request.LicenseNumber.Trim();

        if (!string.IsNullOrWhiteSpace(identityNumber))
        {
            var identityInUse = await dataScope.Coaches()
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
        coach.User.NormalizedUserName = email.ToUpperInvariant();

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
