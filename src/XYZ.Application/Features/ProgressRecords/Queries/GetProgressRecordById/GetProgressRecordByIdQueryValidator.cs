using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById
{
    public class GetProgressRecordByIdQueryValidator
        : AbstractValidator<GetProgressRecordByIdQuery>
    {
        public GetProgressRecordByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Undefinied register id.");
        }
    }
}
