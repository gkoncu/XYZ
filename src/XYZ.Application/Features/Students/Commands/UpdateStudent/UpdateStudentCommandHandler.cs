using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;

        public UpdateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope)
        {
            _context = context;
            _dataScope = dataScope;
        }

        public async Task<int> Handle(UpdateStudentCommand request, CancellationToken ct)
        {
            // 1) Scope + yetki kontrolü: sadece scope içindeki öğrenciyi görebilelim
            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            // Sınıf değişiyorsa, o sınıfa erişim yetkisi var mı kontrol et
            if (request.ClassId.HasValue && request.ClassId != student.ClassId)
                await _dataScope.EnsureClassAccessAsync(request.ClassId.Value, ct);

            var user = student.User ?? throw new InvalidOperationException("Öğrencinin bağlı kullanıcısı bulunamadı.");

            // 2) E-posta değişmişse, UserName ve normalized alanları ile birlikte güncelle
            if (!string.IsNullOrWhiteSpace(request.Email) &&
                !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var normalized = request.Email.ToUpperInvariant();

                // Aynı tenant içinde e-posta çakışmasını engelle
                var emailInTenant = await _context.Users
                    .AnyAsync(u => u.TenantId == user.TenantId &&
                                   u.Id != user.Id &&
                                   u.NormalizedEmail == normalized, ct);

                if (emailInTenant)
                    throw new InvalidOperationException("Bu e-posta adresi bu tenant içinde zaten kullanılıyor.");

                user.Email = request.Email;
                user.UserName = request.Email;
                user.NormalizedEmail = normalized;
                user.NormalizedUserName = normalized;
            }

            // 3) Telefon değişmişse güncelle
            if (!string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal))
            {
                user.PhoneNumber = request.PhoneNumber;
            }

            // 4) User alanları (IdentityUser’daki ekstra alanlar)
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BirthDate = request.BirthDate;
            user.Gender = Enum.Parse<Gender>(request.Gender, true);
            user.BloodType = Enum.Parse<BloodType>(request.BloodType, true);

            // 5) Student alanları
            student.ClassId = request.ClassId;
            student.IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber)
                ? null
                : request.IdentityNumber.Trim();

            student.Address = string.IsNullOrWhiteSpace(request.Address)
                ? null
                : request.Address.Trim();

            student.Parent1FirstName = string.IsNullOrWhiteSpace(request.Parent1FirstName)
                ? null
                : request.Parent1FirstName.Trim();

            student.Parent1LastName = string.IsNullOrWhiteSpace(request.Parent1LastName)
                ? null
                : request.Parent1LastName.Trim();

            student.Parent1Email = string.IsNullOrWhiteSpace(request.Parent1Email)
                ? null
                : request.Parent1Email.Trim();

            student.Parent1PhoneNumber = string.IsNullOrWhiteSpace(request.Parent1PhoneNumber)
                ? null
                : request.Parent1PhoneNumber.Trim();

            student.Parent2FirstName = string.IsNullOrWhiteSpace(request.Parent2FirstName)
                ? null
                : request.Parent2FirstName.Trim();

            student.Parent2LastName = string.IsNullOrWhiteSpace(request.Parent2LastName)
                ? null
                : request.Parent2LastName.Trim();

            student.Parent2Email = string.IsNullOrWhiteSpace(request.Parent2Email)
                ? null
                : request.Parent2Email.Trim();

            student.Parent2PhoneNumber = string.IsNullOrWhiteSpace(request.Parent2PhoneNumber)
                ? null
                : request.Parent2PhoneNumber.Trim();

            student.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

            student.MedicalInformation = string.IsNullOrWhiteSpace(request.MedicalInformation)
                ? null
                : request.MedicalInformation.Trim();

            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
