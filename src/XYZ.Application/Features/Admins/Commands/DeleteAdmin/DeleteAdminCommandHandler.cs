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

namespace XYZ.Application.Features.Admins.Commands.DeleteAdmin
{
    public class DeleteAdminCommandHandler : IRequestHandler<DeleteAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _current = currentUser;
            _userManager = userManager;
        }

        public async Task<int> Handle(DeleteAdminCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Admin silme yetkiniz yok.");

            var tenantId = _current.TenantId;

            var q = _context.Admins.AsQueryable();

            if (role == "Admin")
            {
                if (!tenantId.HasValue)
                    throw new UnauthorizedAccessException("TenantId bulunamadı.");

                q = q.Where(a => a.TenantId == tenantId.Value);
            }

            if (role == "Admin" && tenantId.HasValue)
            {
                var activeAdminCount = await _context.Admins
                    .CountAsync(a => a.TenantId == tenantId.Value && a.IsActive, ct);

                if (activeAdminCount <= 1)
                    throw new InvalidOperationException("Tenant içinde son admin silinemez.");
            }

            var admin = await q.FirstOrDefaultAsync(a => a.Id == request.AdminId, ct);
            if (admin is null)
                throw new NotFoundException("Admin", request.AdminId);

            var userId = admin.UserId;

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync(ct);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is not null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        var rmRole = await _userManager.RemoveFromRoleAsync(user, "Admin");
                        if (!rmRole.Succeeded)
                        {
                            var msg = string.Join("; ", rmRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
                            throw new InvalidOperationException($"Kullanıcı rolü kaldırılamadı (Admin): {msg}");
                        }
                    }

                    var hasStudentProfile = await _context.Students.AnyAsync(s => s.UserId == user.Id, ct);
                    var hasCoachProfile = await _context.Coaches.AnyAsync(c => c.UserId == user.Id, ct);

                    if (!hasStudentProfile && !hasCoachProfile)
                    {
                        var del = await _userManager.DeleteAsync(user);
                        if (!del.Succeeded)
                        {
                            var msg = string.Join("; ", del.Errors.Select(e => $"{e.Code}:{e.Description}"));
                            throw new InvalidOperationException($"Kullanıcı silinemedi: {msg}");
                        }
                    }
                }
            }

            return request.AdminId;
        }
    }
}
