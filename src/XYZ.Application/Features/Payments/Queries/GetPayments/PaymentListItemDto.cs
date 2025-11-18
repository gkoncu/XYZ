using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPayments
{
    public class PaymentListItemDto
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string StudentFullName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public PaymentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
