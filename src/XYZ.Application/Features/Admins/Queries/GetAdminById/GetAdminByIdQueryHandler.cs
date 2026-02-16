using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Admins.Queries.GetAdminById
{
    public sealed class GetAdminByIdQueryHandler
        : IRequestHandler<GetAdminByIdQuery, AdminDetailDto?>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDetailDto?> Handle(
            GetAdminByIdQuery request,
            CancellationToken cancellationToken)
        {
            var admin = await _context.Admins
                .Include(a => a.User)
                .Include(a => a.Tenant)
                .FirstOrDefaultAsync(a => a.Id == request.AdminId, cancellationToken);

            if (admin is null) return null;

            return new AdminDetailDto
            {
                Id = admin.Id,
                UserId = admin.UserId,
                FullName = admin.User.FullName,
                Email = admin.User.Email ?? string.Empty,
                PhoneNumber = admin.User.PhoneNumber,

                Gender = admin.User.Gender.ToString(),
                BloodType = admin.User.BloodType.ToString(),
                BirthDate = admin.User.BirthDate,

                TenantId = admin.TenantId,
                TenantName = admin.Tenant.Name,
                ProfilePictureUrl = admin.User.ProfilePictureUrl,

                IdentityNumber = admin.IdentityNumber ?? string.Empty,
                CanManageUsers = admin.CanManageUsers,
                CanManageFinance = admin.CanManageFinance,
                CanManageSettings = admin.CanManageSettings,

                IsActive = admin.IsActive && admin.User.IsActive,
                CreatedAt = admin.CreatedAt,
                UpdatedAt = admin.UpdatedAt
            };
        }
    }
}
