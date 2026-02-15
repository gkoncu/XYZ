using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Common.Models;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPayments;

public sealed class GetPaymentsQuery : IRequest<PaginationResult<PaymentListItemDto>>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Students.PaymentsRead;
    public PermissionScope? MinimumScope => PermissionScope.Self;

    public int? StudentId { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateOnly? FromDueDate { get; set; }
    public DateOnly? ToDueDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
