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

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDataScopeService _dataScope;


        public CreateStudentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IDataScopeService dataScope)
        {
            _context = context;
            _currentUser = currentUser;
            _dataScope = dataScope;
        }


        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId ?? throw new UnauthorizedAccessException("TenantId not found.");


            var @class = request.ClassId.HasValue
            ? await _dataScope.GetScopedClasses().FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken)
            : null;


            if (@class is null && request.ClassId.HasValue)
                throw new UnauthorizedAccessException("Bu sınıfa erişiminiz yok.");


            var appUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = Enum.Parse<Gender>(request.Gender),
                BloodType = Enum.Parse<BloodType>(request.BloodType),
                BirthDate = request.BirthDate,
                Branch = @class?.Branch.ToString() ?? string.Empty,
                TenantId = tenantId,
                IsActive = true
            };


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
                User = appUser
            };


            await _context.Students.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);


            return student.Id;
        }
    }
}
