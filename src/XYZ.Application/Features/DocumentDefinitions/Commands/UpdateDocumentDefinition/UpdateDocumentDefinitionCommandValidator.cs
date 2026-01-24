using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition
{
    public class UpdateDocumentDefinitionCommandValidator : AbstractValidator<UpdateDocumentDefinitionCommand>
    {
        public UpdateDocumentDefinitionCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }
}
