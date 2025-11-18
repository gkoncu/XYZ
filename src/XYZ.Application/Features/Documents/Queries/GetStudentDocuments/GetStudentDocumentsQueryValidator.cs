using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Documents.Queries.GetStudentDocuments
{
    public class GetStudentDocumentsQueryValidator
        : AbstractValidator<GetStudentDocumentsQuery>
    {
        public GetStudentDocumentsQueryValidator()
        {
            RuleFor(x => x.StudentId).GreaterThan(0);
        }
    }
}
