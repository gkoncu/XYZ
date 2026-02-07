using FluentValidation;
using System;
using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Admins.Commands.UpdateAdmin
{
    public sealed class UpdateAdminCommandValidator : AbstractValidator<UpdateAdminCommand>
    {
        private static readonly PhoneAttribute PhoneAttribute = new();

        public UpdateAdminCommandValidator()
        {
            RuleFor(x => x.AdminId).GreaterThan(0);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.PhoneNumber)
                .Must(p => PhoneAttribute.IsValid(p))
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("PhoneNumber format is invalid.");

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, true, out _))
                .WithMessage("Invalid Gender value.");

            RuleFor(x => x.BloodType)
                .NotEmpty()
                .Must(v => Enum.TryParse<BloodType>(v, true, out _))
                .WithMessage("Invalid BloodType value.");

            RuleFor(x => x.BirthDate)
                .Must(d =>
                {
                    var today = DateTime.Today;
                    var bd = d.Date;
                    return bd <= today &&
                           bd <= today.AddYears(-16) &&
                           bd >= today.AddYears(-100);
                })
                .WithMessage("BirthDate must be a valid date (age between 16 and 100, not in the future).");

            RuleFor(x => x.IdentityNumber)
                .Matches(@"^\d{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("IdentityNumber must be exactly 11 digits.");
        }
    }
}
