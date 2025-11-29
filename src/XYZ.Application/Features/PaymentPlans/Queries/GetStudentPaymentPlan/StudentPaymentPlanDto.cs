using System;
using System.Collections.Generic;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan
{
    public class StudentPaymentPlanDto
    {
        public int PlanId { get; set; }
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public int TotalInstallments { get; set; }
        public DateTime FirstDueDate { get; set; }
        public bool IsInstallment { get; set; }
        public PaymentPlanStatus Status { get; set; }

        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public List<StudentPaymentInstallmentDto> Installments { get; set; } = new();
    }

    public class StudentPaymentInstallmentDto
    {
        public int PaymentId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
