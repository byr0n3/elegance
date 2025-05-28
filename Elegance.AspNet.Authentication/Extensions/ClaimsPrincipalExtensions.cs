using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Elegance.AspNet.Authentication.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		private static ClaimsIdentity Identity(this ClaimsPrincipal @this)
		{
			foreach (var identity in @this.Identities)
			{
				if (identity.IsAuthenticated)
				{
					return identity;
				}
			}

			Debug.Assert(@this.Identity is not null);

			return (ClaimsIdentity)@this.Identity;
		}

		public static bool TryGetClaimValue(this ClaimsPrincipal @this, string claimType, [NotNullWhen(true)] out string? result)
		{
			foreach (var claim in @this.Identity().Claims)
			{
				if (string.Equals(claim.Type, claimType, System.StringComparison.Ordinal))
				{
					result = claim.Value;
					return true;
				}
			}

			result = null;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetClaimValue<T>(this ClaimsPrincipal @this, string claimType, [NotNullWhen(true)] out T? result)
			where T : System.ISpanParsable<T>
		{
			if (!@this.TryGetClaimValue(claimType, out var value))
			{
				result = default;
				return false;
			}

			return T.TryParse(value, null, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string? GetClaimValue(this ClaimsPrincipal @this, string claimType) =>
			@this.TryGetClaimValue(claimType, out var result) ? result : null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T? GetClaimValue<T>(this ClaimsPrincipal @this, string claimType)
			where T : System.ISpanParsable<T> =>
			@this.TryGetClaimValue<T>(claimType, out var result) ? result : default;
	}
}
