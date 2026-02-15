using MediatR;
using XYZ.Application.Common.Interfaces;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;

namespace XYZ.Application.Features.Payments.Commands.DeletePayment;

public sealed class DeletePaymentCommand : IRequest<int>, IRequirePermission
{
    public string PermissionKey => PermissionNames.Payments.Adjust;
    public PermissionScope? MinimumScope => PermissionScope.Tenant;

    public int Id { get; set; }
}
