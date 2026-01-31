using DocumentFormat.OpenXml.ExtendedProperties;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XYZ.Domain.Enums;

public class CreatePaymentCommand : IRequest<int>, IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Öğrenci seçimi geçersiz.")]
    public int StudentId { get; set; }

    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [DataType(DataType.Currency)]
    public decimal? DiscountAmount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date;

    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
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

        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
            yield return new ValidationResult("Not en fazla 500 karakter olabilir.", new[] { nameof(Notes) });

        var today = DateTime.UtcNow.Date;
        if (DueDate.Date < today.AddDays(-365) || DueDate.Date > today.AddDays(365))
            yield return new ValidationResult("Vade tarihi 1 yıl geçmişe/ileriye gidemez.", new[] { nameof(DueDate) });
    }
}
