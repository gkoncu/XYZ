using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Application.Features.Auth.Refresh.Commands
{
    public sealed record RefreshCommand(
    string RefreshToken,
    string? CreatedByIp = null,
    string? UserAgent = null
) : IRequest<DTOs.LoginResultDto>;
}
