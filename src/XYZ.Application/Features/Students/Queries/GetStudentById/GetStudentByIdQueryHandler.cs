using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Students.Queries.GetStudentById
{
    public class GetStudentByIdQueryHandler
        : IRequestHandler<GetStudentByIdQuery, StudentDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetStudentByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<StudentDetailDto> Handle(
            GetStudentByIdQuery request,
            CancellationToken ct)
        {
            var dto = await _dataScope.Students()
                .Where(s => s.Id == request.StudentId)
                .Select(s => new StudentDetailDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email ?? string.Empty,
                    PhoneNumber = s.User.PhoneNumber,

                    Gender = s.User.Gender.ToString(),
                    BloodType = s.User.BloodType.ToString(),
                    BirthDate = s.User.BirthDate,

                    TenantId = s.TenantId,

                    ProfilePictureUrl = s.User.ProfilePictureUrl,
                    ClassId = s.ClassId,
                    ClassName = s.Class != null ? s.Class.Name : null,
                    BranchId = s.Class != null ? (int?)s.Class.BranchId : null,
                    BranchName = s.Class != null ? s.Class.Branch.Name : null,

                    IdentityNumber = s.IdentityNumber ?? string.Empty,
                    Address = s.Address,

                    Parent1FirstName = s.Parent1FirstName,
                    Parent1LastName = s.Parent1LastName,
                    Parent1Email = s.Parent1Email,
                    Parent1PhoneNumber = s.Parent1PhoneNumber,

                    Parent2FirstName = s.Parent2FirstName,
                    Parent2LastName = s.Parent2LastName,
                    Parent2Email = s.Parent2Email,
                    Parent2PhoneNumber = s.Parent2PhoneNumber,

                    MedicalInformation = s.MedicalInformation,
                    Notes = s.Notes,

                    IsActive = s.IsActive && s.User.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(ct);

            if (dto is null)
                throw new NotFoundException("Student", request.StudentId);

            return dto;
        }
    }
}
