using Microsoft.AspNetCore.Authorization;

namespace ISHealthMonitor.UI.Auth
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public AdminRequirement() { }
    }
}
