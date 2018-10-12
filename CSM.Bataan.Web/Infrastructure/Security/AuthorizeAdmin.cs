using CSM.Bataan.Web.Infrastructure.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSM.Bataan.Web.Infrastructure.Security
{
    public class AuthorizeAdminRequirementHandler : AuthorizationHandler<AuthorizeAdminRequirement>
    {
        public AuthorizeAdminRequirementHandler() { }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeAdminRequirement requirement)
        {
            if (!WebUser.Roles.Contains(Role.Admin))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class AuthorizeAdminRequirement : IAuthorizationRequirement
    {
    }
}
