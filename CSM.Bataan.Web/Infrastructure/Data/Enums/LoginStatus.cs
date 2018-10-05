using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Infrastructure.Data.Enums
{
    public enum LoginStatus
    {
        Locked = 0,
        Active = 1,
        NeedsToChangePassword = 2,
        NewRegister = 3
    }
}
