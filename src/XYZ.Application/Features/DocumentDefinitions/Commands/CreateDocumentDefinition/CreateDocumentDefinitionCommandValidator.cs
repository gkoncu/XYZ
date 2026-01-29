using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition
{
    public class CreateDocumentDefinitionCommandValidator : AbstractValidator<CreateDocumentDefinitionCommand>
    {
        public CreateDocumentDefinitionCommandValidator()
        {
            RuleFor(x => x.Target)
                .IsInEnum();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(80);

            RuleFor(x => x.SortOrder)
                .InclusiveBetween(0, 10000);

            RuleFor(x => x.IsActive)
                .NotNull();
        }
    }
}
