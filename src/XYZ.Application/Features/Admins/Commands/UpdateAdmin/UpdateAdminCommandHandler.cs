using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommandHandler : IRequestHandler<UpdateAdminCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UpdateAdminCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(UpdateAdminCommand request, CancellationToken cancellationToken)
        {
            var role = _current.Role ?? string.Empty;
            var tenantId = _current.TenantId;

            var q = _context.Admins
                .Include(a => a.User)
                .AsQueryable();

            switch (role)
            {
                case "SuperAdmin":
                    break;

                case "Admin":
                    if (tenantId > 0)
                        q = q.Where(a => a.TenantId == tenantId);
                    else
                        throw new UnauthorizedAccessException("TenantId is missing.");
                    break;

                default:
                    throw new UnauthorizedAccessException("You are not allowed to update admins.");
            }

            var admin = await q.FirstOrDefaultAsync(a => a.Id == request.AdminId, cancellationToken);
            if (admin is null)
                throw new KeyNotFoundException("Admin not found.");

            var email = (request.Email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Email is required.");

            var identityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
                ? null
                : request.IdentityNumber.Trim();

            if (!string.IsNullOrWhiteSpace(identityNumber))
            {
                var dup = await _context.Admins.AnyAsync(
                    a => a.TenantId == admin.TenantId
                         && a.Id != admin.Id
                         && a.IdentityNumber == identityNumber,
                    cancellationToken);

                if (dup)
                    throw new InvalidOperationException("IdentityNumber is already used in this tenant.");
            }

            if (!Enum.TryParse<Gender>(request.Gender ?? string.Empty, true, out var gender))
                throw new InvalidOperationException("Invalid Gender value.");

            if (!Enum.TryParse<BloodType>(request.BloodType ?? string.Empty, true, out var bloodType))
                throw new InvalidOperationException("Invalid BloodType value.");

            admin.User.FirstName = (request.FirstName ?? string.Empty).Trim();
            admin.User.LastName = (request.LastName ?? string.Empty).Trim();
            admin.User.Email = email;
            admin.User.UserName = email;
            admin.User.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            admin.User.Gender = gender;
            admin.User.BloodType = bloodType;
            admin.User.BirthDate = request.BirthDate.Date;

            admin.User.NormalizedEmail = email.ToUpperInvariant();
            admin.User.NormalizedUserName = email.ToUpperInvariant();
            admin.User.UpdatedAt = DateTime.UtcNow;

            admin.IdentityNumber = identityNumber;
            admin.CanManageUsers = request.CanManageUsers;
            admin.CanManageFinance = request.CanManageFinance;
            admin.CanManageSettings = request.CanManageSettings;
            admin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return admin.Id;
        }
    }
}
