using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord
{
    public class DeleteProgressRecordCommandValidator
        : AbstractValidator<DeleteProgressRecordCommand>
    {
        public DeleteProgressRecordCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
