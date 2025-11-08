using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ.Domain.Enums;

namespace XYZ.Application.Common.Interfaces.Auth
{
    public interface IPasswordSignIn
    {
        Task<PasswordSignInResult> CheckAsync(
            string userId,
            string password,
            CancellationToken ct = default);
    }

    public sealed record PasswordSignInResult(
        PasswordSignInStatus Status
    )
    {
        public bool Succeeded => Status == PasswordSignInStatus.Success;
    }
}
