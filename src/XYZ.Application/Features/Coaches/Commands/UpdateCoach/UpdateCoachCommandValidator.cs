using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Gender)
                .NotEmpty();

            RuleFor(x => x.BloodType)
                .NotEmpty();

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.UtcNow.AddYears(-3));

            RuleFor(x => x.IdentityNumber)
                .Length(11)
                .Matches("^[0-9]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber));

            RuleFor(x => x.LicenseNumber)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.LicenseNumber));

            RuleFor(x => x.BranchId)
                .GreaterThan(0);
        }
    }
}
