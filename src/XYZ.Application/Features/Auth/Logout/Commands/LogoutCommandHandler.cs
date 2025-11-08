using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces.Auth;

namespace XYZ.Application.Features.Auth.Logout.Commands
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IRefreshTokenStore _rtStore;

        public LogoutCommandHandler(IRefreshTokenStore rtStore)
        {
            _rtStore = rtStore;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken ct)
        {
            await _rtStore.RevokeAsync(request.RefreshToken, ct);
            return true;
        }
    }
}
