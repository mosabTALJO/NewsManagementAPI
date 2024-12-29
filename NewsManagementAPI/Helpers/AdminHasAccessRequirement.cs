using Microsoft.AspNetCore.Authorization;

namespace NewsManagementAPI.Helpers
{
    public class AdminHasAccessRequirement : IAuthorizationRequirement
    {
        public string RequiredPolicy { get; set; }
        public AdminHasAccessRequirement(string requiredPolicy)
        {
            RequiredPolicy = requiredPolicy;
        }
    }
    public class AdminHasAccessHandler : AuthorizationHandler<AdminHasAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminHasAccessRequirement requirement)
        {
            if (context.User?.Identity?.Name != null)
            {
                if (context.User.HasClaim(x => x.Type == requirement.RequiredPolicy))
                    context.Succeed(requirement);
                else
                    context.Fail();
            }
            else
                context.Fail();
            return Task.CompletedTask;
        }
    }
}