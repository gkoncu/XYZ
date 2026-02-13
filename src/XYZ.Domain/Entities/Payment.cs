using System;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class Payment : TenantScopedEntity
    {
        public int StudentId { get; set; }

        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }

        public PaymentStatus Status { get; set; }

        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }

        public string? DiscountReason { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int InstallmentNumber { get; set; } = 1;
        public int TotalInstallments { get; set; } = 1;
        public bool IsInstallment { get; set; } = false;
        public string? InstallmentPlan { get; set; }
        public int? PaymentPlanId { get; set; }
        public PaymentPlan? PaymentPlan { get; set; }

        public Student Student { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
