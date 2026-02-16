using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Queries.GetPaymentById;

public sealed class GetPaymentByIdQuery : IRequest<PaymentDetailDto>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Students.PaymentsRead;
    public PermissionScope? MinimumScope => PermissionScope.Self;

    public int Id { get; set; }
}
