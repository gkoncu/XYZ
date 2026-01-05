using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _current;

        public CreateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _context = context;
            _dataScope = dataScope;
            _current = currentUser;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new InvalidOperationException("UserId zorunludur.");

            var tenantId = _current.TenantId ?? throw new UnauthorizedAccessException("TenantId bulunamadı.");

            if (request.ClassId.HasValue)
                await _dataScope.EnsureClassAccessAsync(request.ClassId.Value, ct);

            var exists = await _context.Students.AnyAsync(s => s.UserId == request.UserId, ct);
            if (exists)
                throw new InvalidOperationException("Bu kullanıcı için zaten Student profili oluşturulmuş.");

            if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                var identityInTenant = await _context.Students
                    .AnyAsync(s => s.TenantId == tenantId && s.IdentityNumber == request.IdentityNumber, ct);

                if (identityInTenant)
                    throw new InvalidOperationException("TC Kimlik No bu tenant içinde zaten kullanılıyor.");
            }

            var student = new Student
            {
                UserId = request.UserId,
                TenantId = tenantId,
                ClassId = request.ClassId,

                IdentityNumber = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : request.IdentityNumber.Trim(),
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
                Notes = request.Notes,

                IsActive = true
            };

            await _context.Students.AddAsync(student, ct);
            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
