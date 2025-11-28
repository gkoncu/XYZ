using MediatR;
using System;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommand : IRequest<int>
    {
        public int StudentId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalInstallments { get; set; }
        public DateTime FirstDueDate { get; set; }
        public bool IsInstallment { get; set; }
    }
}
