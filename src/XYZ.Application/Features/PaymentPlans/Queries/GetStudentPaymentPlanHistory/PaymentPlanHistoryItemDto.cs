using System;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory
{
    public class PaymentPlanHistoryItemDto
    {
        public int PlanId { get; set; }
        public DateTime CreatedAt { get; set; }

        public string PlanName { get; set; } = string.Empty;
        public PaymentPlanStatus Status { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }

        public int TotalInstallments { get; set; }
        public DateTime FirstDueDate { get; set; }
        public bool IsInstallment { get; set; }
    }
}
