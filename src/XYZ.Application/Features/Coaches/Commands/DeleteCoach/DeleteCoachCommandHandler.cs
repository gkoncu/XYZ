using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach
{
    public class DeleteCoachCommandHandler : IRequestHandler<DeleteCoachCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteCoachCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
            _userManager = userManager;
        }

        public async Task<int> Handle(DeleteCoachCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Koç silme yetkiniz yok.");

            var coach = await _dataScope.Coaches()
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, ct);

            if (coach is null)
                throw new NotFoundException("Coach", request.CoachId);

            var userId = coach.UserId;

            _context.Coaches.Remove(coach);
            await _context.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Coach"))
                    {
                        var rmRole = await _userManager.RemoveFromRoleAsync(user, "Coach");
                        if (!rmRole.Succeeded)
                        {
                            var msg = string.Join("; ", rmRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
                            throw new InvalidOperationException($"Kullanıcı rolü kaldırılamadı (Coach): {msg}");
                        }
                    }

                    var hasStudentProfile = await _context.Students.AnyAsync(s => s.UserId == user.Id, ct);
                    var hasAdminProfile = await _context.Admins.AnyAsync(a => a.UserId == user.Id, ct);

                    if (!hasStudentProfile && !hasAdminProfile)
                    {
                        var del = await _userManager.DeleteAsync(user);
                        if (!del.Succeeded)
                        {
                            var msg = string.Join("; ", del.Errors.Select(e => $"{e.Code}:{e.Description}"));
                            throw new InvalidOperationException($"Kullanıcı silinemedi: {msg}");
                        }
                    }
                    else
                    {
                    }
                }
            }

            return request.CoachId;
        }
    }
}
