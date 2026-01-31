using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPaymentById;

public sealed class PaymentDetailDto
{
    public int Id { get; set; }

    public int? PaymentPlanId { get; set; }

    public int StudentId { get; set; }

    public string StudentFullName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public decimal? DiscountAmount { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
