using FluentValidation;

namespace XYZ.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
    {
        public CreateDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentDefinitionId)
                .GreaterThan(0);

            RuleFor(x => x)
                .Must(x => ((x.StudentId ?? 0) > 0) ^ ((x.CoachId ?? 0) > 0))
                .WithMessage("Exactly one of StudentId or CoachId must be provided.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(80);

            RuleFor(x => x.FilePath)
                .NotEmpty()
                .MaximumLength(500);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
