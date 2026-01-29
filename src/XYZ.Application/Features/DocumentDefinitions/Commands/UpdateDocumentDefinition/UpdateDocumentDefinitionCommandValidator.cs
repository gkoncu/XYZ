using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition
{
    public class UpdateDocumentDefinitionCommandValidator : AbstractValidator<UpdateDocumentDefinitionCommand>
    {
        public UpdateDocumentDefinitionCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

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
