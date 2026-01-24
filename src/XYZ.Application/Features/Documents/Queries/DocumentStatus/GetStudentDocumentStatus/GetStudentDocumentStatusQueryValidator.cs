using FluentValidation;

namespace XYZ.Application.Features.Documents.Queries.DocumentStatus.GetStudentDocumentStatus
{
    public class GetStudentDocumentStatusQueryValidator : AbstractValidator<GetStudentDocumentStatusQuery>
    {
        public GetStudentDocumentStatusQueryValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
