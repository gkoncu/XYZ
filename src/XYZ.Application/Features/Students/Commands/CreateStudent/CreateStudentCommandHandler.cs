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
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _dataScope = dataScope;
            _currentUser = currentUser;
            _userManager = userManager;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken ct)
        {
            var role = _currentUser.Role;
            if (role is null || (role != "Admin" && role != "Coach" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Öğrenci oluşturma yetkiniz yok.");

            var tenantId = _currentUser.TenantId
                ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            if (request.ClassId.HasValue)
                await _dataScope.EnsureClassAccessAsync(request.ClassId.Value, ct);

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var normalized = _userManager.NormalizeEmail(request.Email);
                var emailInTenant = await _context.Users
                    .AnyAsync(u => u.TenantId == tenantId && u.NormalizedEmail == normalized, ct);

                if (emailInTenant)
                    throw new InvalidOperationException("Bu e-posta adresi bu tenant içinde zaten kullanılıyor.");
            }

            if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                var identityInTenant = await _context.Students
                    .AnyAsync(s => s.TenantId == tenantId && s.IdentityNumber == request.IdentityNumber, ct);

                if (identityInTenant)
                    throw new InvalidOperationException("TC Kimlik No bu tenant içinde zaten kullanılıyor.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                Gender = Enum.Parse<Gender>(request.Gender, true),
                BloodType = Enum.Parse<BloodType>(request.BloodType, true),
                TenantId = tenantId,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var msg = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {msg}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Student");
            if (!roleResult.Succeeded)
            {
                var msg = string.Join("; ", roleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Rol atanamadı (Student): {msg}");
            }

            var student = new Student
            {
                UserId = user.Id,
                TenantId = tenantId,
                ClassId = request.ClassId,

                IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null: request.IdentityNumber.Trim(),
                Address = request.Address,

                Parent1FirstName = request.Parent1FirstName,
                Parent1LastName = request.Parent1LastName,
                Parent1Email = request.Parent1Email,
                Parent1PhoneNumber = request.Parent1PhoneNumber,

                Parent2FirstName = request.Parent2FirstName,
                Parent2LastName = request.Parent2LastName,
                Parent2Email = request.Parent2Email,
                Parent2PhoneNumber = request.Parent2PhoneNumber,

                MedicalInformation = request.MedicalInformation,
                Notes = request.Notes
            };

            await _context.Students.AddAsync(student, ct);
            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
