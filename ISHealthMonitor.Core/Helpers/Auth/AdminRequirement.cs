using Microsoft.AspNetCore.Authorization;

namespace ISHealthMonitor.Core.Helpers.Auth
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public AdminRequirement() { }
    }
}
