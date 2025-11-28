using System;
using System.Collections.Generic;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class PaymentPlan : BaseEntity
    {
        public int StudentId { get; set; }
        public int TenantId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalInstallments { get; set; }
        public DateTime FirstDueDate { get; set; }
        public bool IsInstallment { get; set; }
        public string? Name { get; set; }

        public string? Notes { get; set; }

        public PaymentPlanStatus Status { get; set; } = PaymentPlanStatus.Active;

        public Student Student { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
