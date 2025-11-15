using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.UpdateCoach
{
    public class UpdateCoachCommandHandler : IRequestHandler<UpdateCoachCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateCoachCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateCoachCommand request, CancellationToken cancellationToken)
        {
            var coach = await _dataScope.Coaches()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, cancellationToken);

            if (coach is null)
                throw new NotFoundException("Coach", request.CoachId);

            if (coach.BranchId != request.BranchId)
            {
                var branchOk = await _context.Branches
                    .AnyAsync(b =>
                        b.Id == request.BranchId &&
                        b.TenantId == coach.TenantId,
                        cancellationToken);

                if (!branchOk)
                    throw new UnauthorizedAccessException("Bu branşa erişiminiz yok.");
            }

            var gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true);
            var bloodType = Enum.Parse<BloodType>(request.BloodType, ignoreCase: true);

            coach.User.FirstName = request.FirstName.Trim();
            coach.User.LastName = request.LastName.Trim();
            coach.User.Email = request.Email;
            coach.User.UserName = request.Email;
            coach.User.PhoneNumber = request.PhoneNumber;
            coach.User.Gender = gender;
            coach.User.BloodType = bloodType;
            coach.User.BirthDate = request.BirthDate;
            coach.User.UpdatedAt = DateTime.UtcNow;

            coach.IdentityNumber = request.IdentityNumber ?? string.Empty;
            coach.LicenseNumber = request.LicenseNumber ?? string.Empty;
            coach.BranchId = request.BranchId;
            coach.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return coach.Id;
        }
    }
}
