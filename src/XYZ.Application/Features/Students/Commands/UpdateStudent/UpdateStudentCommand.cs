using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommand : IRequest<StudentDto>
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalInformation { get; set; }
        public int? ClassId { get; set; }
        public int CoachId { get; set; }
    }

    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataScopeService _dataScopeService;

        public UpdateStudentCommandHandler(IApplicationDbContext context, IMapper mapper, IDataScopeService dataScopeService)
        {
            _context = context;
            _mapper = mapper;
            _dataScopeService = dataScopeService;
        }

        public async Task<StudentDto> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _dataScopeService.GetScopedStudents()
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (student == null)
            {
                throw new NotFoundException(nameof(Student), request.Id);
            }

            // Verify coach exists and user has access
            var coach = await _dataScopeService.GetScopedCoaches()
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, cancellationToken);

            if (coach == null)
            {
                throw new NotFoundException(nameof(Coach), request.CoachId);
            }

            // Verify class exists if provided
            if (request.ClassId.HasValue)
            {
                var classEntity = await _dataScopeService.GetScopedClasses()
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken);

                if (classEntity == null)
                {
                    throw new NotFoundException(nameof(Class), request.ClassId.Value);
                }
            }

            student.FirstName = request.FirstName;
            student.LastName = request.LastName;
            student.PhoneNumber = request.PhoneNumber;
            student.BirthDate = request.BirthDate;
            student.ParentName = request.ParentName;
            student.ParentPhone = request.ParentPhone;
            student.EmergencyContact = request.EmergencyContact;
            student.MedicalInformation = request.MedicalInformation;
            student.ClassId = request.ClassId;
            student.CoachId = request.CoachId;
            student.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Reload with includes for complete DTO
            var updatedStudent = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .FirstOrDefaultAsync(s => s.Id == student.Id, cancellationToken);

            return _mapper.Map<StudentDto>(updatedStudent);
        }
    }

    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Student ID is required.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .LessThan(DateTime.Today).WithMessage("Birth date must be in the past.");

            RuleFor(x => x.CoachId)
                .GreaterThan(0).WithMessage("Coach is required.");
        }
    }
}
