using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScopeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UpdateStudentCommandHandler> _logger;

        public UpdateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScopeService,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            ILogger<UpdateStudentCommandHandler> logger)
        {
            _context = context;
            _dataScopeService = dataScopeService;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating student with ID: {StudentId}", request.UpdateStudentRequest.Id);

                var student = await GetStudentWithAuthorization(request.UpdateStudentRequest.Id);

                await ValidateEmailUniqueness(request.UpdateStudentRequest.Email, student.UserId);

                if (request.UpdateStudentRequest.ClassId.HasValue &&
                    request.UpdateStudentRequest.ClassId != student.ClassId)
                {
                    await ValidateClassAccess(request.UpdateStudentRequest.ClassId.Value);
                }

                await UpdateApplicationUser(student.User, request.UpdateStudentRequest);

                await UpdateStudentEntity(student, request.UpdateStudentRequest);

                _logger.LogInformation("Successfully updated student with ID: {StudentId}", student.Id);

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating student with ID: {StudentId}",
                    request.UpdateStudentRequest.Id);
                throw;
            }
        }

        private async Task<Student> GetStudentWithAuthorization(int studentId)
        {
            var canAccess = await _dataScopeService.CanAccessStudentAsync(studentId);
            if (!canAccess)
            {
                throw new UnauthorizedAccessException($"You don't have permission to update student with ID {studentId}.");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                throw new ArgumentException($"Student with ID {studentId} not found.");
            }

            return student;
        }

        private async Task ValidateEmailUniqueness(string newEmail, string currentUserId)
        {
            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != currentUserId)
            {
                throw new ArgumentException($"Email {newEmail} is already taken by another user.");
            }
        }

        private async Task ValidateClassAccess(int classId)
        {
            var currentUserRole = _currentUserService.Role;

            if (currentUserRole == "Coach")
            {
                var canAccessClass = await _dataScopeService.CanAccessClassAsync(classId);
                if (!canAccessClass)
                {
                    throw new UnauthorizedAccessException("You don't have permission to assign students to this class.");
                }
            }

            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == classId && c.TenantId == _currentUserService.TenantId);

            if (!classExists)
            {
                throw new ArgumentException($"Class with ID {classId} not found.");
            }
        }

        private async Task UpdateApplicationUser(ApplicationUser user, UpdateStudentRequest request)
        {
            BloodType bloodType = BloodType.Unknown;
            if (!string.IsNullOrEmpty(request.BloodType) && Enum.TryParse<BloodType>(request.BloodType, out var parsedBloodType))
            {
                bloodType = parsedBloodType;
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.BirthDate = request.BirthDate;
            user.BloodType = bloodType;
            user.Branch = request.Branch;

            if (user.Email != request.Email)
            {
                user.Email = request.Email;
                user.UserName = request.Email;
                user.NormalizedEmail = request.Email.ToUpper();
                user.NormalizedUserName = request.Email.ToUpper();
            }

            user.PhoneNumber = request.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User update failed: {errors}");
            }
        }

        private async Task UpdateStudentEntity(Student student, UpdateStudentRequest request)
        {
            student.Parent1FirstName = request.Parent1FirstName;
            student.Parent1LastName = request.Parent1LastName;
            student.Parent1PhoneNumber = request.Parent1PhoneNumber;
            student.Parent1Email = request.Parent1Email;
            student.Parent2FirstName = request.Parent2FirstName;
            student.Parent2LastName = request.Parent2LastName;
            student.Parent2PhoneNumber = request.Parent2PhoneNumber;
            student.Parent2Email = request.Parent2Email;
            student.Address = request.Address;
            student.ClassId = request.ClassId;
            student.IdentityNumber = request.IdentityNumber;
            student.UpdatedAt = DateTime.UtcNow;

            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }
    }
}
