using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Payments.Commands.UpdatePayment
{
    public class UpdatePaymentCommandHandler
        : IRequestHandler<UpdatePaymentCommand, int>
    {
        private readonly IDataScopeService _dataScope;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _current;

        public UpdatePaymentCommandHandler(
            IDataScopeService dataScope,
            IApplicationDbContext context,
            ICurrentUserService currentUser)
        {
            _dataScope = dataScope;
            _context = context;
            _current = currentUser;
        }

        public async Task<int> Handle(UpdatePaymentCommand request, CancellationToken ct)
        {
            var role = _current.Role;
            if (role is null || (role != "Admin" && role != "SuperAdmin"))
                throw new UnauthorizedAccessException("Ödeme güncelleme yetkiniz yok.");

            var payment = await _dataScope.Payments()
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

            if (payment is null)
                throw new NotFoundException("Payment", request.Id);

            payment.Amount = request.Amount;
            payment.DiscountAmount = request.DiscountAmount;
            payment.Status = request.Status;

            if (request.IsActive.HasValue)
                payment.IsActive = request.IsActive.Value;

            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return payment.Id;
        }
    }
}
