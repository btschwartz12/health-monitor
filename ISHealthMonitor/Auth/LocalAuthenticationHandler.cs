using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;



namespace ISHealthMonitor.Core.Helpers.Auth
{
   
    public class LocalAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public LocalAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Create a claim with a dummy user and identity.
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "localuser") };
            var identity = new ClaimsIdentity(claims, "local");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "local");

            // Return successful authentication result.
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

}
