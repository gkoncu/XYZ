using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition
{
    public class CreateDocumentDefinitionCommandValidator : AbstractValidator<CreateDocumentDefinitionCommand>
    {
        public CreateDocumentDefinitionCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }
}
