using MediatR;
using System;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan
{
    public class CreatePaymentPlanCommand : IRequest<int>, IRequirePermission
    {
        public string PermissionKey => PermissionNames.Payments.CreatePlan;
        public PermissionScope? MinimumScope => PermissionScope.Tenant;

        public int StudentId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalInstallments { get; set; }
        public DateTime FirstDueDate { get; set; }
        public bool IsInstallment { get; set; }
    }
}
