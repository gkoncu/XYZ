using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Features.Students.DTOs;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommand : IRequest<int>
    {
        public CreateStudentRequest CreateStudentRequest { get; set; } = null!;
    }

    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(x => x.CreateStudentRequest)
                .NotNull().WithMessage("CreateStudentRequest is required.");

            RuleFor(x => x.CreateStudentRequest.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(50).WithMessage("FirstName cannot exceed 50 characters.");

            RuleFor(x => x.CreateStudentRequest.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(50).WithMessage("LastName cannot exceed 50 characters.");

            RuleFor(x => x.CreateStudentRequest.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.CreateStudentRequest.BirthDate)
                .NotEmpty().WithMessage("BirthDate is required.")
                .LessThan(DateTime.Now).WithMessage("BirthDate must be in the past.");

            When(x => !string.IsNullOrEmpty(x.CreateStudentRequest.Parent1PhoneNumber), () =>
            {
                RuleFor(x => x.CreateStudentRequest.Parent1PhoneNumber)
                    .MaximumLength(20).WithMessage("Parent1 phone number cannot exceed 20 characters.");
            });

            When(x => !string.IsNullOrEmpty(x.CreateStudentRequest.Parent2PhoneNumber), () =>
            {
                RuleFor(x => x.CreateStudentRequest.Parent2PhoneNumber)
                    .MaximumLength(20).WithMessage("Parent2 phone number cannot exceed 20 characters.");
            });

            RuleFor(x => x.CreateStudentRequest.ClassId)
                .GreaterThan(0).When(x => x.CreateStudentRequest.ClassId.HasValue)
                .WithMessage("Class ID must be greater than 0.");
        }
    }
}
