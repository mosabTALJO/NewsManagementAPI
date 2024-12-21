using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

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
        private readonly IMemoryCache _memoryCache;
        public AdminHasAccessHandler(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminHasAccessRequirement requirement)
        {
            if (context.User?.Identity?.Name != null)
            {
                if (_memoryCache.TryGetValue(context.User.Identity.Name, out string[]? claims) && claims?.Contains(requirement.RequiredPolicy) == true)
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
