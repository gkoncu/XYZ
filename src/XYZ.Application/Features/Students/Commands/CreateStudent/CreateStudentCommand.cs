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
    public class CreateStudentCommand : IRequest<int>
    {
        public CreateStudentRequest CreateStudentRequest { get; set; } = null!;
    }


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


            var @class = request.CreateStudentRequest.ClassId.HasValue
            ? await _dataScope.GetScopedClasses().FirstOrDefaultAsync(c => c.Id == request.CreateStudentRequest.ClassId.Value, cancellationToken)
            : null;


            if (@class is null && request.CreateStudentRequest.ClassId.HasValue)
                throw new UnauthorizedAccessException("Bu sınıfa erişiminiz yok.");


            var appUser = new ApplicationUser
            {
                UserName = request.CreateStudentRequest.Email,
                Email = request.CreateStudentRequest.Email,
                PhoneNumber = request.CreateStudentRequest.PhoneNumber,
                FirstName = request.CreateStudentRequest.FirstName,
                LastName = request.CreateStudentRequest.LastName,
                Gender = Enum.Parse<Gender>(request.CreateStudentRequest.Gender),
                BloodType = Enum.Parse<BloodType>(request.CreateStudentRequest.BloodType),
                BirthDate = request.CreateStudentRequest.BirthDate,
                Branch = @class?.Branch ?? string.Empty,
                TenantId = tenantId,
                IsActive = true
            };


            var student = new Student
            {
                Address = request.CreateStudentRequest.Address,
                IdentityNumber = request.CreateStudentRequest.IdentityNumber,
                ClassId = request.CreateStudentRequest.ClassId,
                TenantId = tenantId,
                Parent1FirstName = request.CreateStudentRequest.Parent1FirstName,
                Parent1LastName = request.CreateStudentRequest.Parent1LastName,
                Parent1Email = request.CreateStudentRequest.Parent1Email,
                Parent1PhoneNumber = request.CreateStudentRequest.Parent1PhoneNumber,
                Parent2FirstName = request.CreateStudentRequest.Parent2FirstName,
                Parent2LastName = request.CreateStudentRequest.Parent2LastName,
                Parent2Email = request.CreateStudentRequest.Parent2Email,
                Parent2PhoneNumber = request.CreateStudentRequest.Parent2PhoneNumber,
                MedicalInformation = request.CreateStudentRequest.MedicalInformation,
                Notes = request.CreateStudentRequest.Notes,
                User = appUser
            };


            await _context.Students.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);


            return student.Id;
        }
    }
}
