using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);

            RuleFor(x => x.FirstName).NotEmpty().MinimumLength(2).MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MinimumLength(2).MaximumLength(100);

            RuleFor(x => x.Email)
                .MaximumLength(256)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.UtcNow.Date)
                .GreaterThan(new DateTime(1900, 1, 1));

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, true, out _))
                .WithMessage("Geçerli bir cinsiyet değeri giriniz.");

            RuleFor(x => x.BloodType)
                .NotEmpty()
                .Must(v => Enum.TryParse<BloodType>(v, true, out _))
                .WithMessage("Geçerli bir kan grubu değeri giriniz.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0).When(x => x.ClassId.HasValue);

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("TC Kimlik No 11 haneli olmalıdır.");

            RuleFor(x => x.Parent1Email)
                .EmailAddress()
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1Email));

            RuleFor(x => x.Parent2Email)
                .EmailAddress()
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2Email));

            RuleFor(x => x.Parent1PhoneNumber)
                .MaximumLength(32)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent1PhoneNumber));

            RuleFor(x => x.Parent2PhoneNumber)
                .MaximumLength(32)
                .When(x => !string.IsNullOrWhiteSpace(x.Parent2PhoneNumber));

            RuleFor(x => x.Address).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Address));
            RuleFor(x => x.Notes).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
            RuleFor(x => x.MedicalInformation).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.MedicalInformation));
        }
    }
}
