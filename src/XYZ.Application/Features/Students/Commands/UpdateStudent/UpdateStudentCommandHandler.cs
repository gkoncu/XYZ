using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IDataScopeService _dataScope;

        public UpdateStudentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUser,
            IDataScopeService dataScope)
        {
            _context = context;
            _currentUser = currentUser;
            _dataScope = dataScope;
        }

        public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _dataScope.GetScopedStudents()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

            if (student is null)
                throw new Exception("Öğrenci bulunamadı veya erişim izniniz yok.");

            student.User.FirstName = request.FirstName;
            student.User.LastName = request.LastName;
            student.User.PhoneNumber = request.PhoneNumber;
            student.User.Email = request.Email;
            student.User.Gender = Enum.Parse<Gender>(request.Gender);
            student.User.BloodType = Enum.Parse<BloodType>(request.BloodType);
            student.User.BirthDate = request.BirthDate;
            student.User.ProfilePictureUrl = request.ProfilePictureUrl;
            student.User.IsActive = request.IsActive;

            if (request.ClassId.HasValue)
            {
                var targetClass = await _dataScope.GetScopedClasses()
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken);

                if (targetClass is null)
                    throw new UnauthorizedAccessException("Bu sınıfa erişiminiz yok.");

                student.ClassId = request.ClassId;
            }
            else
            {
                student.ClassId = null;
            }

            student.IdentityNumber = request.IdentityNumber;
            student.Address = request.Address;
            student.Parent1FirstName = request.Parent1FirstName;
            student.Parent1LastName = request.Parent1LastName;
            student.Parent1Email = request.Parent1Email;
            student.Parent1PhoneNumber = request.Parent1PhoneNumber;
            student.Parent2FirstName = request.Parent2FirstName;
            student.Parent2LastName = request.Parent2LastName;
            student.Parent2Email = request.Parent2Email;
            student.Parent2PhoneNumber = request.Parent2PhoneNumber;
            student.MedicalInformation = request.MedicalInformation;
            student.Notes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
