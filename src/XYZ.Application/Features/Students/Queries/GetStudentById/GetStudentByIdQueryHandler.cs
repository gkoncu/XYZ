using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Students.Queries.GetStudentById
{
    public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public GetStudentByIdQueryHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
        {
            var student = await _dataScope.GetScopedStudents()
                .Include(s => s.User)
                .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (student is null)
                throw new UnauthorizedAccessException("Bu öğrenciye erişiminiz yok.");

            return new StudentDto
            {
                Id = student.Id,
                FullName = $"{student.User.FirstName} {student.User.LastName}",
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber,
                IdentityNumber = student.IdentityNumber,
                Address = student.Address,
                Gender = student.User.Gender.ToString(),
                BloodType = student.User.BloodType.ToString(),
                Branch = student.User.Branch,
                ClassName = student.Class?.Name,
                IsActive = student.User.IsActive,
                Parent1FullName = $"{student.Parent1FirstName} {student.Parent1LastName}".Trim(),
                Parent2FullName = $"{student.Parent2FirstName} {student.Parent2LastName}".Trim(),
                Parent1Email = student.Parent1Email,
                Parent2Email = student.Parent2Email,
                Notes = student.Notes,
                MedicalInformation = student.MedicalInformation
            };
        }
    }
}
