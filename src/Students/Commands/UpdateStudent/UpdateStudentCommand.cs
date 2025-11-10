using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommand : IRequest<Unit>
    {
        public UpdateStudentRequest UpdateStudentRequest { get; set; } = null!;
    }

    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.UpdateStudentRequest)
                .NotNull().WithMessage("UpdateStudentRequest is required.");

            RuleFor(x => x.UpdateStudentRequest.Id)
                .GreaterThan(0).WithMessage("Student ID must be greater than 0.");

            RuleFor(x => x.UpdateStudentRequest.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");

            RuleFor(x => x.UpdateStudentRequest.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");

            RuleFor(x => x.UpdateStudentRequest.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.UpdateStudentRequest.BirthDate)
                .NotEmpty().WithMessage("BirthDate is required.")
                .LessThan(DateTime.Now).WithMessage("BirthDate must be in the past.");

            When(x => !string.IsNullOrEmpty(x.UpdateStudentRequest.Parent1PhoneNumber), () =>
            {
                RuleFor(x => x.UpdateStudentRequest.Parent1PhoneNumber)
                    .MaximumLength(20).WithMessage("Parent1 phone number cannot exceed 20 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.UpdateStudentRequest.Parent2PhoneNumber), () =>
            {
                RuleFor(x => x.UpdateStudentRequest.Parent2PhoneNumber)
                    .MaximumLength(20).WithMessage("Parent2 phone number cannot exceed 20 characters.");
            });

            RuleFor(x => x.UpdateStudentRequest.ClassId)
                .GreaterThan(0).When(x => x.UpdateStudentRequest.ClassId.HasValue)
                .WithMessage("Class ID must be greater than 0.");
        }
    }
}
