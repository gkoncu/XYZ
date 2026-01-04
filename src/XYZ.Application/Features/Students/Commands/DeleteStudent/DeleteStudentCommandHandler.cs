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

namespace XYZ.Application.Features.Students.Commands.DeleteStudent
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteStudentCommandHandler(
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

        public async Task<int> Handle(DeleteStudentCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Öğrenci silme yetkiniz yok.");

            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            student.IsActive = false;
            student.UpdatedAt = DateTime.UtcNow;

            var user = student.User;
            if (user is not null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student"))
                {
                    var rmRole = await _userManager.RemoveFromRoleAsync(user, "Student");
                    if (!rmRole.Succeeded)
                    {
                        var msg = string.Join("; ", rmRole.Errors.Select(e => $"{e.Code}:{e.Description}"));
                        throw new InvalidOperationException($"Kullanıcı rolü kaldırılamadı (Student): {msg}");
                    }
                }

                var hasCoachProfile = await _context.Coaches.AnyAsync(c => c.UserId == user.Id, ct);
                var hasAdminProfile = await _context.Admins.AnyAsync(a => a.UserId == user.Id, ct);

                if (!hasCoachProfile && !hasAdminProfile)
                {
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(ct);

            return request.StudentId;
        }
    }
}
