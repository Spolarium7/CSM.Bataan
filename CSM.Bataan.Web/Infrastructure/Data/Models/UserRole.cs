using CSM.Bataan.Web.Infrastructure.Data.Enums;
using CSM.Bataan.Web.Infrastructure.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Infrastructure.Data.Models
{
    public class UserRole: BaseModel
    {
        public Guid? UserId { get; set; }

        public Role Role { get; set; } 
    }
}
