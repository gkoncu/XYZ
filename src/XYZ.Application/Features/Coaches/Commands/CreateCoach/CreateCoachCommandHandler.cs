using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.CreateCoach
{
    public class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public CreateCoachCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(CreateCoachCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.Id == request.BranchId &&
                    b.TenantId == tenantId,
                    cancellationToken);

            if (branch is null)
                throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");

            var gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true);
            var bloodType = Enum.Parse<BloodType>(request.BloodType, ignoreCase: true);

            var appUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = gender,
                BloodType = bloodType,
                BirthDate = request.BirthDate,
                TenantId = tenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var coach = new Coach
            {
                TenantId = tenantId,
                BranchId = request.BranchId,
                IdentityNumber = request.IdentityNumber ?? string.Empty,
                LicenseNumber = request.LicenseNumber ?? string.Empty,
                User = appUser,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Coaches.Add(coach);
            await _context.SaveChangesAsync(cancellationToken);

            return coach.Id;
        }
    }
}
