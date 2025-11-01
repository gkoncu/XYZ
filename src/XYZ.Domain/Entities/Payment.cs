using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Common;
using XYZ.Domain.Enums;

namespace XYZ.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int StudentId { get; set; }
        public int TenantId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public string? DiscountReason { get; set; }
        public decimal? DiscountAmount { get; set; }

        public Student Student { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
