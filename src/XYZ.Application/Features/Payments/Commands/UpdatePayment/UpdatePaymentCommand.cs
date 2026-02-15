using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Commands.UpdatePayment;

public sealed class UpdatePaymentCommand : IRequest<int>, IRequirePermission, IValidatableObject
{
    public string PermissionKey => PermissionNames.Payments.Adjust;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;

    [Range(1, int.MaxValue, ErrorMessage = "Ödeme seçimi geçersiz.")]
    public int Id { get; set; }

    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [DataType(DataType.Currency)]
    public decimal? DiscountAmount { get; set; }

    public PaymentStatus Status { get; set; }

    public bool? IsActive { get; set; }

    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
            yield return new ValidationResult("Not en fazla 500 karakter olabilir.", new[] { nameof(Notes) });

        if (Amount <= 0)
            yield return new ValidationResult("Tutar 0'dan büyük olmalıdır.", new[] { nameof(Amount) });

        if (Amount > 99999)
            yield return new ValidationResult("Tutar 99.999'dan büyük olamaz.", new[] { nameof(Amount) });

        if (DiscountAmount.HasValue)
        {
            if (DiscountAmount.Value < 0)
                yield return new ValidationResult("İndirim tutarı 0'dan küçük olamaz.", new[] { nameof(DiscountAmount) });

            if (DiscountAmount.Value > Amount)
                yield return new ValidationResult("İndirim tutarı tutardan büyük olamaz.", new[] { nameof(DiscountAmount) });
        }

        if (!Enum.IsDefined(typeof(PaymentStatus), Status))
            yield return new ValidationResult("Durum değeri geçersiz.", new[] { nameof(Status) });
    }
}
