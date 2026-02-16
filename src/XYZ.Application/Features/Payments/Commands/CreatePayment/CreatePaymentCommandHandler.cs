using MediatR;
using Microsoft.EntityFrameworkCore;
using XYZ.Application.Common.Exceptions;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Entities;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Commands.CreatePayment;

public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IDataScopeService _dataScope;

    public CreatePaymentCommandHandler(IApplicationDbContext context, IDataScopeService dataScope)
    {
        _context = context;
        _dataScope = dataScope;
    }

    public async Task<int> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        var studentExists = await _dataScope.Students()
            .AnyAsync(s => s.Id == request.StudentId, ct);

        if (!studentExists)
            throw new NotFoundException("Student", request.StudentId);

        var now = DateTime.UtcNow;

        var entity = new Payment
        {
            StudentId = request.StudentId,
            Amount = request.Amount,
            DiscountAmount = request.DiscountAmount,
            Notes = request.Notes,
            Status = request.Status,
            DueDate = request.DueDate,
            PaidDate = request.Status == PaymentStatus.Paid ? now : null,
            IsActive = true
        };

        _context.Payments.Add(entity);
        await _context.SaveChangesAsync(ct);

        return entity.Id;
    }
}
