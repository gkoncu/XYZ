using MediatR;
using Microsoft.AspNetCore.Identity;
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
    public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationDbContext _db;
        private readonly IDataScopeService _dataScope;
        private readonly ICurrentUserService _currentUser;

        public CreateStudentCommandHandler(
            UserManager<ApplicationUser> userManager,
            IApplicationDbContext db,
            IDataScopeService dataScope,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _db = db;
            _dataScope = dataScope;
            _currentUser = currentUser;
        }

        public async Task<string> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _currentUser.TenantId;
            if (tenantId is null)
                throw new InvalidOperationException("TenantId could not be resolved.");

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true),
                BloodType = Enum.Parse<BloodType>(request.BloodType, ignoreCase: true),
                Branch = request.Branch,
                BirthDate = request.BirthDate,
                UserName = request.Email,
                TenantId = int.Parse(tenantId),
                IsActive = true
            };

            var identityResult = await _userManager.CreateAsync(user);
            if (!identityResult.Succeeded)
                throw new ApplicationException(string.Join("; ", identityResult.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, "Student");

            var student = new Student
            {
                UserId = user.Id,
                TenantId = int.Parse(tenantId),
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
                MedicalInformation = request.MedicalInformation,
                Notes = request.Notes
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
