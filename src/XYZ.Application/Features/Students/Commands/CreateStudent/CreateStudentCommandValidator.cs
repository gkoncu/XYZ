using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad alanı zorunludur.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad alanı zorunludur.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("Doğum tarihi zorunludur.")
                .LessThan(DateTime.Today).WithMessage("Doğum tarihi bugünden ileri olamaz.");

            RuleFor(x => x.Gender)
                .Must(value => Enum.TryParse<Gender>(value, out _))
                .WithMessage("Geçerli bir cinsiyet seçimi yapılmalıdır.");

            RuleFor(x => x.BloodType)
                .Must(value => Enum.TryParse<BloodType>(value, out _))
                .WithMessage("Geçerli bir kan grubu seçilmelidir.");

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$").When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("T.C. Kimlik Numarası 11 haneli olmalıdır.");

            RuleFor(x => x.ClassId)
                .GreaterThan(0).When(x => x.ClassId.HasValue)
                .WithMessage("Geçerli bir sınıf ID'si belirtilmelidir.");
        }
    }
}
