using FluentValidation;

namespace XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById
{
    public class GetClassSessionByIdQueryValidator
        : AbstractValidator<GetClassSessionByIdQuery>
    {
        public GetClassSessionByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}
