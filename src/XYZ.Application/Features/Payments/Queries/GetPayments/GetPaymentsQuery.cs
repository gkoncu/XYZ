using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPayments
{
    public class GetPaymentsQuery
        : IRequest<PaginationResult<PaymentListItemDto>>
    {
        public int? StudentId { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateOnly? FromDueDate { get; set; }
        public DateOnly? ToDueDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
