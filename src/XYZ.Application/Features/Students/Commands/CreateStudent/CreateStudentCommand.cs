using AutoMapper;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Students.DTOs;
using XYZ.Domain.Entities;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommand : IRequest<StudentDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalInformation { get; set; }
        public int? ClassId { get; set; }
        public int CoachId { get; set; }
    }

    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataScopeService _dataScopeService;

        public CreateStudentCommandHandler(IApplicationDbContext context, IMapper mapper, IDataScopeService dataScopeService)
        {
            _context = context;
            _mapper = mapper;
            _dataScopeService = dataScopeService;
        }

        public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            var coach = await _dataScopeService.GetScopedCoaches()
                .FirstOrDefaultAsync(c => c.Id == request.CoachId, cancellationToken);

            if (coach == null)
            {
                throw new NotFoundException(nameof(Coach), request.CoachId);
            }

            if (request.ClassId.HasValue)
            {
                var classEntity = await _dataScopeService.GetScopedClasses()
                    .FirstOrDefaultAsync(c => c.Id == request.ClassId.Value, cancellationToken);

                if (classEntity == null)
                {
                    throw new NotFoundException(nameof(Class), request.ClassId.Value);
                }
            }

            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                ParentName = request.ParentName,
                ParentPhone = request.ParentPhone,
                EmergencyContact = request.EmergencyContact,
                MedicalInformation = request.MedicalInformation,
                ClassId = request.ClassId,
                CoachId = request.CoachId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload with includes for complete DTO
            var createdStudent = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Coach)
                .FirstOrDefaultAsync(s => s.Id == student.Id, cancellationToken);

            return _mapper.Map<StudentDto>(createdStudent);
        }
    }

    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

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
