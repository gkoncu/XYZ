using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Features.Payments.Queries.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDetailDto>
{
    private readonly IDataScopeService _dataScope;

    public GetPaymentByIdQueryHandler(IDataScopeService dataScope)
    {
        _dataScope = dataScope;
    }

    public async Task<PaymentDetailDto> Handle(GetPaymentByIdQuery request, CancellationToken ct)
    {
        var dto = await _dataScope.Payments()
            .Include(p => p.Student)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new PaymentDetailDto
            {
                Id = p.Id,
                PaymentPlanId = p.PaymentPlanId,
                StudentId = p.StudentId,
                StudentFullName = p.Student.User.FullName,
                Amount = p.Amount,
                DiscountAmount = p.DiscountAmount,
                Status = p.Status,
                DueDate = p.DueDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            throw new KeyNotFoundException("Ödeme bulunamadı.");

        return dto;
    }
}
