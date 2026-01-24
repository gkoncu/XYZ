using FluentValidation;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetCoachDocumentStatus
{
    public class GetCoachDocumentStatusQueryValidator : AbstractValidator<GetCoachDocumentStatusQuery>
    {
        public GetCoachDocumentStatusQueryValidator()
        {
            RuleFor(x => x.CoachId).GreaterThan(0);
        }
    }
}
