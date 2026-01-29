using FluentValidation;

namespace XYZ.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
    {
        public UpdateDocumentCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(80);

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.FilePath)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.FilePath));
        }
    }
}
