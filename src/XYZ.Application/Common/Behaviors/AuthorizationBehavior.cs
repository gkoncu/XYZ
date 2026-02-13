using MediatR;
using XYZ.Application.Common.Interfaces;

namespace XYZ.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _current;
    private readonly IPermissionService _permissions;

    public AuthorizationBehavior(ICurrentUserService current, IPermissionService permissions)
    {
        _current = current;
        _permissions = permissions;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IRequirePermission req)
            return await next();

        if (_current.IsAuthenticated != true)
            throw new UnauthorizedAccessException("Giriş yapmadan bu işlem yapılamaz.");

        var ok = await _permissions.HasPermissionAsync(req.PermissionKey, req.MinimumScope, ct);
        if (!ok)
            throw new UnauthorizedAccessException("Bu işlem için yetkiniz yok.");

        return await next();
    }
}
