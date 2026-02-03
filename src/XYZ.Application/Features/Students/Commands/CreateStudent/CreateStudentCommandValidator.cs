using FluentValidation;

namespace XYZ.Application.Features.Students.Commands.CreateStudent
{
    public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

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
