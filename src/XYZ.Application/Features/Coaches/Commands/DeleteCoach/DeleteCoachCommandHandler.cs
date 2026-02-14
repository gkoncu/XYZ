using MediatR;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Coaches.Commands.DeleteCoach;

public sealed class DeleteCoachCommandHandler(
    IDataScopeService dataScope,
    IApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : IRequestHandler<DeleteCoachCommand, int>
{
    public async Task<int> Handle(DeleteCoachCommand request, CancellationToken ct)
    {
        var coach = await dataScope.Coaches()
            .FirstOrDefaultAsync(c => c.Id == request.CoachId, ct);

        if (coach is null)
            throw new NotFoundException("Coach", request.CoachId);

        var userId = coach.UserId;

        context.Coaches.Remove(coach);
        await context.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is not null)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains(RoleNames.Coach))
                {
                    var rmRole = await userManager.RemoveFromRoleAsync(user, RoleNames.Coach);
                    if (!rmRole.Succeeded)
                    {
                        var msg = string.Join("; ", rmRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Kullanıcı rolü kaldırılamadı ({RoleNames.Coach}): {msg}");
                    }
                }

                var hasStudentProfile = await context.Students.AnyAsync(s => s.UserId == user.Id, ct);
                var hasAdminProfile = await context.Admins.AnyAsync(a => a.UserId == user.Id, ct);

                if (!hasStudentProfile && !hasAdminProfile)
                {
                    var del = await userManager.DeleteAsync(user);
                    if (!del.Succeeded)
                    {
                        var msg = string.Join("; ", del.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Kullanıcı silinemedi: {msg}");
                    }
                }
            }
        }

        return request.CoachId;
    }
}
