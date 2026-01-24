using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.DeleteDocumentDefinition
{
    public class DeleteDocumentDefinitionCommandValidator : AbstractValidator<DeleteDocumentDefinitionCommand>
    {
        public DeleteDocumentDefinitionCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
