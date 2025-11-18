using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Payments.Queries.GetPaymentById
{
    public class GetPaymentByIdQueryHandler
        : IRequestHandler<GetPaymentByIdQuery, PaymentDetailDto>
    {
        private readonly IDataScopeService _dataScope;

        public GetPaymentByIdQueryHandler(IDataScopeService dataScope)
        {
            _dataScope = dataScope;
        }

        public async Task<PaymentDetailDto> Handle(
            GetPaymentByIdQuery request,
            CancellationToken ct)
        {
            var payment = await _dataScope.Payments()
                .Include(p => p.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (payment is null)
                throw new NotFoundException("Payment", request.Id);

            return new PaymentDetailDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId,
                StudentId = payment.StudentId,
                StudentFullName = payment.Student.User.FullName,
                Amount = payment.Amount,
                DiscountAmount = payment.DiscountAmount,
                Status = payment.Status,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}
