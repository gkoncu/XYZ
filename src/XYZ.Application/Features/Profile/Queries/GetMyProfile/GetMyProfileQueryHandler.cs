using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Profile.Queries.GetMyProfile;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MyProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDataScopeService _dataScope;
    private readonly ICurrentUserService _current;

    public GetMyProfileQueryHandler(
        IApplicationDbContext context,
        IDataScopeService dataScope,
        ICurrentUserService current)
    {
        _context = context;
        _dataScope = dataScope;
        _current = current;
    }

    public async Task<MyProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        if (!_current.IsAuthenticated || string.IsNullOrWhiteSpace(_current.UserId))
            throw new UnauthorizedAccessException("Kullanıcı doğrulanamadı.");

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Tenant)
            .Include(u => u.StudentProfile)
                .ThenInclude(s => s!.Class)
                    .ThenInclude(c => c!.Branch)
            .Include(u => u.CoachProfile)
                .ThenInclude(c => c!.Branch)
            .FirstOrDefaultAsync(u => u.Id == _current.UserId, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        var dto = new MyProfileDto
        {
            UserId = user.Id,
            Role = _current.Role ?? string.Empty,

            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name,

            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,

            FirstName = user.FirstName,
            LastName = user.LastName,

            Gender = user.Gender,
            BloodType = user.BloodType,
            BirthDate = user.BirthDate,

            ProfilePictureUrl = user.ProfilePictureUrl,

            StudentProfileId = user.StudentProfile?.Id,
            CoachProfileId = user.CoachProfile?.Id
        };

        if (dto.Role == "Student" && user.StudentProfile?.Class is not null)
        {
            dto.ClassName = user.StudentProfile.Class.Name;
            dto.BranchName = user.StudentProfile.Class.Branch?.Name;
        }

        if (dto.Role == "Coach" && user.CoachProfile is not null)
        {
            dto.BranchName = user.CoachProfile.Branch?.Name;
        }

        return dto;
    }
}
