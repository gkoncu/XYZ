using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition
{
    public class UpdateProgressMetricDefinitionCommandValidator : AbstractValidator<UpdateProgressMetricDefinitionCommand>
    {
        public UpdateProgressMetricDefinitionCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Geçersiz metrik id.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Şube seçimi zorunludur.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Metrik adı zorunludur.")
                .MaximumLength(120).WithMessage("Metrik adı en fazla 120 karakter olabilir.");

            RuleFor(x => x.Unit)
                .MaximumLength(32).WithMessage("Birim en fazla 32 karakter olabilir.");

            RuleFor(x => x.SortOrder)
                .InclusiveBetween(0, 10000).WithMessage("Sıralama 0-10000 aralığında olmalı.");

            RuleFor(x => x.DataType)
                .IsInEnum().WithMessage("Geçersiz veri tipi.");

            RuleFor(x => x)
                .Must(x => !x.MinValue.HasValue || !x.MaxValue.HasValue || x.MinValue.Value <= x.MaxValue.Value)
                .WithMessage("Min değer, Max değerden büyük olamaz.");

            RuleFor(x => x)
                .Must(x => x.DataType != ProgressMetricDataType.Text || (!x.MinValue.HasValue && !x.MaxValue.HasValue))
                .WithMessage("Metin tipi metriklerde Min/Max değer kullanılmaz.");
        }
    }
}
