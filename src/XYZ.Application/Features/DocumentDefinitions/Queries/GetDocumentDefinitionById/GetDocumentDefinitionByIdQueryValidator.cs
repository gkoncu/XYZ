using FluentValidation;

namespace XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById
{
    public class GetDocumentDefinitionByIdQueryValidator : AbstractValidator<GetDocumentDefinitionByIdQuery>
    {
        public GetDocumentDefinitionByIdQueryValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
