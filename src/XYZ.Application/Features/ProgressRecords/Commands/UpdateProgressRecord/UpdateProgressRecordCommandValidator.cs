using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord
{
    public class UpdateProgressRecordCommandValidator
        : AbstractValidator<UpdateProgressRecordCommand>
    {
        public UpdateProgressRecordCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçersiz kayıt id.");

            RuleForEach(x => x.Values).ChildRules(v =>
            {
                v.RuleFor(x => x.ProgressMetricDefinitionId)
                    .GreaterThan(0).WithMessage("Geçersiz metrik seçimi.");

                v.RuleFor(x => x)
                    .Must(HasExactlyOneValue)
                    .WithMessage("Her metrik için yalnızca bir değer girilebilir (Sayısal / Tam sayı / Metin).");
            });
        }

        private static bool HasExactlyOneValue(MetricValueInput v)
        {
            var filled =
                (v.DecimalValue.HasValue ? 1 : 0) +
                (v.IntValue.HasValue ? 1 : 0) +
                (!string.IsNullOrWhiteSpace(v.TextValue) ? 1 : 0);

            return filled <= 1;
        }
    }
}
