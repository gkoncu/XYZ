using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Auth.Logout.Commands
{
    public sealed record LogoutCommand(
    string RefreshToken
) : IRequest<bool>;
}
