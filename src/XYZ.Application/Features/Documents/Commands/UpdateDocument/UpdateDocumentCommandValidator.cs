using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Documents.Commands.UpdateDocument
{
    public class UpdateDocumentCommandValidator
        : AbstractValidator<UpdateDocumentCommand>
    {
        public UpdateDocumentCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.FilePath)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.FilePath));
        }
    }
}
