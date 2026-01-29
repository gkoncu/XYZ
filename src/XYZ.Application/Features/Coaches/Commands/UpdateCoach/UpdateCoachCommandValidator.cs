using FluentValidation;
using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Coaches.Commands.UpdateCoach
{
    public class UpdateCoachCommandValidator : AbstractValidator<UpdateCoachCommand>
    {
        public UpdateCoachCommandValidator()
        {
            RuleFor(x => x.CoachId)
                .GreaterThan(0);

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

            RuleFor(x => x.Gender)
                .NotEmpty()
                .Must(v => Enum.TryParse<Gender>(v, true, out _))
                .WithMessage("Gender is not a valid value.");

            RuleFor(x => x.BloodType)
                .NotEmpty()
                .Must(v => Enum.TryParse<BloodType>(v, true, out _))
                .WithMessage("BloodType is not a valid value.");

            RuleFor(x => x.BirthDate)
                .NotEmpty()
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
                .Matches("^[0-9]{11}$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber))
                .WithMessage("IdentityNumber must be exactly 11 digits.");

            RuleFor(x => x.LicenseNumber)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.LicenseNumber));

            RuleFor(x => x.BranchId)
                .GreaterThan(0);
        }
    }
}
