using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;

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
            ICurrentUserService current)
        {
            _context = context;
            _dataScope = dataScope;
            _current = current;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken ct)
        {
            var tenantId = _current.TenantId;
            if (!tenantId.HasValue)
                throw new UnauthorizedAccessException("Tenant bilgisi bulunamadı.");

            if (request.ClassId.HasValue)
                await _dataScope.EnsureClassAccessAsync(request.ClassId.Value, ct);

            var student = new Student
            {
                TenantId = tenantId.Value,
                UserId = request.UserId,
                ClassId = request.ClassId,
                IdentityNumber = request.IdentityNumber,
                Address = request.Address,

                Parent1FirstName = request.Parent1FirstName,
                Parent1LastName = request.Parent1LastName,
                Parent1Email = request.Parent1Email,
                Parent1PhoneNumber = request.Parent1PhoneNumber,

                Parent2FirstName = request.Parent2FirstName,
                Parent2LastName = request.Parent2LastName,
                Parent2Email = request.Parent2Email,
                Parent2PhoneNumber = request.Parent2PhoneNumber,

                Notes = request.Notes,
                MedicalInformation = request.MedicalInformation,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
