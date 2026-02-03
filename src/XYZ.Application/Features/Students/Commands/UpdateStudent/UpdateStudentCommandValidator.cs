using FluentValidation;
using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public sealed class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .MaximumLength(256)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.BirthDate)
                .Must(d =>
                {
                    var today = DateTime.UtcNow.Date;
                    var min = today.AddYears(-100);
                    return d.Date <= today && d.Date >= min;
                })
                .WithMessage("BirthDate must be within the last 100 years and cannot be in the future.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, true, out _))
                .WithMessage("Invalid Gender value.");

            RuleFor(x => x.BloodType)
                .NotEmpty()
                .Must(v => Enum.TryParse<BloodType>(v, true, out _))
                .WithMessage("Invalid BloodType value.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0)
                .When(x => x.ClassId.HasValue);

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("TC Kimlik No 11 haneli olmalıdır.");

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));

            RuleFor(x => x.Parent1FirstName)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1FirstName));

            RuleFor(x => x.Parent1LastName)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1LastName));

            RuleFor(x => x.Parent2FirstName)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2FirstName));

            RuleFor(x => x.Parent2LastName)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2LastName));

            RuleFor(x => x.Parent1Email)
                .EmailAddress()
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1Email));

            RuleFor(x => x.Parent2Email)
                .EmailAddress()
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2Email));

            RuleFor(x => x.Parent1PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1PhoneNumber));

            RuleFor(x => x.Parent2PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2PhoneNumber));

            RuleFor(x => x.Notes)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.Notes));

            RuleFor(x => x.MedicalInformation)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrWhiteSpace(x.MedicalInformation));
        }
    }
}
