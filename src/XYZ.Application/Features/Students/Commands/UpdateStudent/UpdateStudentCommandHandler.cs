using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScope,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _dataScope = dataScope;
            _userManager = userManager;
        }

        public async Task<int> Handle(UpdateStudentCommand request, CancellationToken ct)
        {
            var student = await _dataScope.Students()
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId, ct);

            if (student is null)
                throw new NotFoundException("Student", request.StudentId);

            if (request.ClassId.HasValue && request.ClassId != student.ClassId)
                await _dataScope.EnsureClassAccessAsync(request.ClassId.Value, ct);

            var user = student.User;

            if (!string.IsNullOrWhiteSpace(request.Email) && !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmail = await _userManager.SetEmailAsync(user, request.Email);
                if (!setEmail.Succeeded)
                {
                    var msg = string.Join("; ", setEmail.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"E-posta güncellenemedi: {msg}");
                }

                var setUserName = await _userManager.SetUserNameAsync(user, request.Email);
                if (!setUserName.Succeeded)
                {
                    var msg = string.Join("; ", setUserName.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Kullanıcı adı güncellenemedi: {msg}");
                }
            }

            if (user.PhoneNumber != request.PhoneNumber)
            {
                var setPhone = await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber ?? string.Empty);
                if (!setPhone.Succeeded)
                {
                    var msg = string.Join("; ", setPhone.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Telefon güncellenemedi: {msg}");
                }
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BirthDate = request.BirthDate;
            user.Gender = Enum.Parse<Gender>(request.Gender, true);
            user.BloodType = Enum.Parse<BloodType>(request.BloodType, true);

            var userUpdate = await _userManager.UpdateAsync(user);
            if (!userUpdate.Succeeded)
            {
                var msg = string.Join("; ", userUpdate.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Kullanıcı güncellenemedi: {msg}");
            }

            student.ClassId = request.ClassId;
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

            student.Notes = request.Notes;
            student.MedicalInformation = request.MedicalInformation;

            await _context.SaveChangesAsync(ct);

            return student.Id;
        }
    }
}
