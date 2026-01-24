using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;

namespace XYZ.Application.Features.Coaches.Queries.GetCoachById
{
    public class GetCoachByIdQueryHandler
        : IRequestHandler<GetCoachByIdQuery, CoachDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetCoachByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<CoachDetailDto> Handle(
            GetCoachByIdQuery request,
            CancellationToken ct)
        {
            var dto = await _dataScope.Coaches()
                .Where(c => c.Id == request.CoachId)
                .Select(c => new CoachDetailDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FirstName + " " + c.User.LastName,
                    Email = c.User.Email ?? string.Empty,
                    PhoneNumber = c.User.PhoneNumber,

                    Gender = c.User.Gender.ToString(),
                    BloodType = c.User.BloodType.ToString(),
                    BirthDate = c.User.BirthDate,

                    TenantId = c.TenantId,

                    BranchId = c.BranchId,
                    BranchName = c.Branch.Name,

                    IdentityNumber = c.IdentityNumber ?? string.Empty,
                    LicenseNumber = c.LicenseNumber ?? string.Empty,

                    IsActive = c.IsActive && c.User.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,

                    Classes = c.Classes
                        .Select(cls => new CoachClassItemDto
                        {
                            Id = cls.Id,
                            FullName = cls.Name,
                            BranchName = cls.Branch.Name,
                            IsActive = cls.IsActive
                        })
                        .ToList()
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(ct);

            if (dto is null)
                throw new NotFoundException("Coach", request.CoachId);

            return dto;
        }
    }
}
