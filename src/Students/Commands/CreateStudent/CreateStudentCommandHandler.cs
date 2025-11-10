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

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDataScopeService _dataScopeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CreateStudentCommandHandler> _logger;

        public CreateStudentCommandHandler(
            IApplicationDbContext context,
            IDataScopeService dataScopeService,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            ILogger<CreateStudentCommandHandler> logger)
        {
            _context = context;
            _dataScopeService = dataScopeService;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new student with email: {Email}", request.CreateStudentRequest.Email);

                await ValidateUserPermissions(request.CreateStudentRequest.ClassId);

                await ValidateUniqueEmail(request.CreateStudentRequest.Email);

                if (request.CreateStudentRequest.ClassId.HasValue)
                {
                    await ValidateClassAccess(request.CreateStudentRequest.ClassId.Value);
                }

                var user = await CreateApplicationUser(request.CreateStudentRequest);

                var student = await CreateStudentEntity(request.CreateStudentRequest, user.Id);

                _logger.LogInformation("Successfully created student with ID: {StudentId}", student.Id);

                return student.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating student with email: {Email}",
                    request.CreateStudentRequest.Email);
                throw;
            }
        }

        private async Task ValidateUserPermissions(int? classId)
        {
            var currentUserRole = _currentUserService.Role;
            var currentTenantId = _currentUserService.TenantId;

            if (currentTenantId == null)
            {
                throw new UnauthorizedAccessException("User tenant not found.");
            }

            if (currentUserRole == "Coach" && classId.HasValue)
            {
                var canAccessClass = await _dataScopeService.CanAccessClassAsync(classId.Value);
                if (!canAccessClass)
                {
                    throw new UnauthorizedAccessException("You don't have permission to add students to this class.");
                }
            }

            if (currentUserRole != "Admin" && currentUserRole != "Coach")
            {
                throw new UnauthorizedAccessException("Only Admin and Coach can create students.");
            }
        }

        private async Task ValidateUniqueEmail(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new ArgumentException($"Email {email} is already taken.");
            }
        }

        private async Task ValidateClassAccess(int classId)
        {
            var classExists = await _context.Classes
                .AnyAsync(c => c.Id == classId && c.TenantId == _currentUserService.TenantId);

            if (!classExists)
            {
                throw new ArgumentException($"Class with ID {classId} not found.");
            }
        }

        private async Task<ApplicationUser> CreateApplicationUser(CreateStudentRequest request)
        {
            BloodType bloodType = BloodType.Unknown;
            if (!string.IsNullOrEmpty(request.BloodType) && Enum.TryParse<BloodType>(request.BloodType, out var parsedBloodType))
            {
                bloodType = parsedBloodType;
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                BloodType = bloodType,
                Branch = request.Branch,
                TenantId = _currentUserService.TenantId ?? throw new InvalidOperationException("TenantId is required")
            };

            var tempPassword = GenerateTemporaryPassword();

            var result = await _userManager.CreateAsync(user, tempPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User creation failed: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Student");

            return user;
        }

        private async Task<Student> CreateStudentEntity(CreateStudentRequest request, string userId)
        {
            var student = new Student
            {
                UserId = userId,
                TenantId = _currentUserService.TenantId ?? throw new InvalidOperationException("TenantId is required"),
                Parent1FirstName = request.Parent1FirstName,
                Parent1LastName = request.Parent1LastName,
                Parent1PhoneNumber = request.Parent1PhoneNumber,
                Parent1Email = request.Parent1Email,
                Parent2FirstName = request.Parent2FirstName,
                Parent2LastName = request.Parent2LastName,
                Parent2PhoneNumber = request.Parent2PhoneNumber,
                Parent2Email = request.Parent2Email,
                Address = request.Address,
                ClassId = request.ClassId,
                IdentityNumber = request.IdentityNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return student;
        }

        private string GenerateTemporaryPassword()
        {
            return "TempPassword123!";
        }
    }
}
