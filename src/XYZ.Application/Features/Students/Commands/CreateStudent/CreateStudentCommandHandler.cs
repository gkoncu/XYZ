using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDataScopeService _dataScope;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateStudentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IDataScopeService dataScope,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _currentUser = currentUser;
            _dataScope = dataScope;
            _userManager = userManager;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId ?? throw new UnauthorizedAccessException("TenantId not found.");

            var @class = request.ClassId.HasValue
                ? await _dataScope.GetScopedClasses()
                    .Include(c => c.Branch)
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken)
                : null;

            if (@class is null && request.ClassId.HasValue)
                throw new UnauthorizedAccessException("Bu sınıfa erişiminiz yok.");

            if (!Enum.TryParse<Gender>(request.Gender, out var gender))
                throw new ArgumentException("Geçersiz cinsiyet bilgisi.");

            if (!Enum.TryParse<BloodType>(request.BloodType, out var bloodType))
                throw new ArgumentException("Geçersiz kan grubu bilgisi.");

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
                Branch = @class?.Branch.Name ?? string.Empty,
                TenantId = tenantId,
                IsActive = true
            };

            var identityResult = await _userManager.CreateAsync(appUser);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {errors}");
            }

            await _userManager.AddToRoleAsync(appUser, "Student");

            var student = new Student
            {
                Address = request.Address,
                IdentityNumber = request.IdentityNumber,
                ClassId = request.ClassId,
                TenantId = tenantId,
                Parent1FirstName = request.Parent1FirstName,
                Parent1LastName = request.Parent1LastName,
                Parent1Email = request.Parent1Email,
                Parent1PhoneNumber = request.Parent1PhoneNumber,
                Parent2FirstName = request.Parent2FirstName,
                Parent2LastName = request.Parent2LastName,
                Parent2Email = request.Parent2Email,
                Parent2PhoneNumber = request.Parent2PhoneNumber,
                MedicalInformation = request.MedicalInformation,
                Notes = request.Notes,
                UserId = appUser.Id
            };

            await _context.Students.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return student.Id;
        }
    }
}
