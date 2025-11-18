using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandValidator
        : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
