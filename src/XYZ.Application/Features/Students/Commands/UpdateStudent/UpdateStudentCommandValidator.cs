using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Students.Commands.UpdateStudent
{
    public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
    {
        public UpdateStudentCommandValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);

            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PhoneNumber).MaximumLength(20);

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(g => Enum.TryParse<Domain.Enums.Gender>(g, out _))
                .WithMessage("Geçerli bir cinsiyet değeri girilmelidir.");

            RuleFor(x => x.BloodType)
                .NotEmpty()
                .Must(b => Enum.TryParse<Domain.Enums.BloodType>(b, out _))
                .WithMessage("Geçerli bir kan grubu değeri girilmelidir.");

            RuleFor(x => x.IdentityNumber).MaximumLength(11);
            RuleFor(x => x.Address).MaximumLength(250);

            RuleFor(x => x.Parent1Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Parent1Email));
            RuleFor(x => x.Parent2Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Parent2Email));

            RuleFor(x => x.Parent1PhoneNumber).MaximumLength(20);
            RuleFor(x => x.Parent2PhoneNumber).MaximumLength(20);

            RuleFor(x => x.Notes).MaximumLength(1000);
            RuleFor(x => x.MedicalInformation).MaximumLength(1000);
        }
    }
}
