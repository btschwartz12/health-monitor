using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ISHealthMonitor.Core.Helpers.Auth
{
	public static class ClaimsPrincipalExtensions
	{
		public static string GetLogin(this ClaimsPrincipal claimsPrincipal)
		{
			if (claimsPrincipal == null)
				return string.Empty;

			var claim = claimsPrincipal.Claims
				.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier));

			if (claim == null)
				return string.Empty;

			return claim.Value;
		}
		public static string GetNetworkLogon()
		{
			return ClaimsPrincipal.Current.Claims
				.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value;
		}
		public static string GetUserPrincipalName()
		{
			var upn = ClaimsPrincipal.Current.Claims
					.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Name)).Value;
			if (upn == null)
				return string.Empty;

			return upn;
		}
		public static List<string> GetUserGroups(this ClaimsPrincipal claimsPrincipal)
		{
			if (claimsPrincipal == null)
				return null;

			var claim = claimsPrincipal.Claims
			   .Where(c => c.Type.EndsWith("groups")).Select(c => c.Value);

			return claim.ToList();
		}
		public static List<string> GetUserClaimsGroups()
		{
			return ClaimsPrincipal.Current.Claims
				.Where(c => c.Type.EndsWith("groups")).Select(c => c.Value).ToList();
		}

	}
}