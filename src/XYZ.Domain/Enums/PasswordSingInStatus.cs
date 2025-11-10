using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYZ.Domain.Enums
{
    public enum PasswordSignInStatus
    {
        Success = 0,
        InvalidCredentials = 1,
        LockedOut = 2,
        RequiresTwoFactor = 3,
        NotAllowed = 4
    }
}
