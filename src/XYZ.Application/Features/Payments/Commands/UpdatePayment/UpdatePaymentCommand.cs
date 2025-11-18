using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommand : IRequest<int>
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public PaymentStatus Status { get; set; }

        public bool? IsActive { get; set; }
    }
}
