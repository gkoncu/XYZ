using MediatR;
using XYZ.Domain.Enums;

public class CreatePaymentCommand : IRequest<int>
{
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.Date;
}
