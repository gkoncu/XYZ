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
            .FirstOrDefaultAsync(u => u.Id == _current.UserId, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        var dto = new MyProfileDto
        {
            UserId = user.Id,
            Role = _current.Role ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name,
            ProfilePictureUrl = user.ProfilePictureUrl
        };

        if ((_current.Role == "Student") && _current.StudentId.HasValue)
        {
            var student = await _dataScope.Students()
                .AsNoTracking()
                .Include(s => s.Class)
                    .ThenInclude(c => c!.Branch)
                .FirstOrDefaultAsync(s => s.Id == _current.StudentId.Value, ct);

            if (student?.Class is not null)
            {
                dto.ClassId = student.Class.Id;
                dto.ClassName = student.Class.Name;
                dto.BranchId = student.Class.BranchId;
                dto.BranchName = student.Class.Branch?.Name;
            }

            return dto;
        }

        if ((_current.Role == "Coach") && _current.CoachId.HasValue)
        {
            var coach = await _dataScope.Coaches()
                .AsNoTracking()
                .Include(c => c.Branch)
                .Include(c => c.Classes)
                .FirstOrDefaultAsync(c => c.Id == _current.CoachId.Value, ct);

            if (coach is not null)
            {
                dto.BranchId = coach.BranchId;
                dto.BranchName = coach.Branch?.Name;
                dto.ClassesCount = coach.Classes?.Count ?? 0;
            }

            return dto;
        }

        if ((_current.Role == "Admin") && _current.TenantId.HasValue)
        {
            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == _current.UserId && a.TenantId == _current.TenantId.Value, ct);

            if (admin is not null)
            {
                dto.AdminId = admin.Id;
                dto.CanManageUsers = admin.CanManageUsers;
                dto.CanManageFinance = admin.CanManageFinance;
                dto.CanManageSettings = admin.CanManageSettings;
            }

            return dto;
        }

        return dto;
    }
}
