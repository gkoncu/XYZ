using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Auth.Login.Commands
{
    public sealed record LoginCommand(
    string Identifier,
    string Password,
    string? CreatedByIp = null,
    string? UserAgent = null
) : IRequest<DTOs.LoginResultDto>;
}
