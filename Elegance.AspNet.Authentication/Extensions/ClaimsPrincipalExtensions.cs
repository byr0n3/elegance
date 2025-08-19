using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication.Extensions
{
	[PublicAPI]
	public static class ClaimsPrincipalExtensions
	{
		/// <summary>
		/// Retrieves the authenticated identity from the specified claims principal.
		/// </summary>
		/// <param name="this">The claims principal to search for an authenticated identity.</param>
		/// <returns>The first authenticated ClaimsIdentity in the claims principal; otherwise, the default ClaimsIdentity of the claims principal.</returns>
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

		/// <summary>
		/// Tries to get the value of a claim from the specified claims principal.
		/// </summary>
		/// <param name="this">The claims principal to search for the claim.</param>
		/// <param name="claimType">The type of the claim to retrieve.</param>
		/// <param name="result">When this method returns, contains the value of the claim if found; otherwise, null. This parameter is passed uninitialized.</param>
		/// <returns>True if the claim was found and its value has been written to the result parameter; otherwise, false.</returns>
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

		/// <summary>
		/// Attempts to retrieve the value of a claim from the specified claims principal and parse it as a specified type.
		/// </summary>
		/// <param name="this">The claims principal to search for the claim.</param>
		/// <param name="claimType">The type of the claim to retrieve.</param>
		/// <param name="result">When this method returns, contains the parsed value of the claim if found and successfully parsed; otherwise, default(T). This parameter is passed uninitialized.</param>
		/// <typeparam name="T">The type to parse the claim value as. Must implement <see cref="System.ISpanParsable{T}"/>.</typeparam>
		/// <returns>True if the claim was found and its value has been successfully parsed into the result parameter; otherwise, false.</returns>
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

		/// <summary>
		/// Gets the value of a claim from the specified claims principal.
		/// </summary>
		/// <param name="this">The claims principal to search for the claim.</param>
		/// <param name="claimType">The type of the claim to retrieve.</param>
		/// <returns>The value of the claim if found; otherwise, null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string? GetClaimValue(this ClaimsPrincipal @this, string claimType) =>
			@this.TryGetClaimValue(claimType, out var result) ? result : null;

		/// <summary>
		/// Retrieves the value of a claim from the specified claims principal.
		/// </summary>
		/// <param name="this">The claims principal to search for the claim.</param>
		/// <param name="claimType">The type of the claim to retrieve.</param>
		/// <returns>The value of the claim if found; otherwise, null.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T? GetClaimValue<T>(this ClaimsPrincipal @this, string claimType)
			where T : System.ISpanParsable<T> =>
			@this.TryGetClaimValue<T>(claimType, out var result) ? result : default;

		/// <summary>
		/// Retrieves the value of a required claim from the specified claims principal.
		/// </summary>
		/// <param name="this">The claims principal to search for the claim.</param>
		/// <param name="claimType">The type of the claim to retrieve.</param>
		/// <returns>The value of the claim if found; otherwise, throws an exception.</returns>
		/// <exception cref="System.Exception">Thrown when the claim is not found in the claims principal.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetRequiredClaimValue(this ClaimsPrincipal @this, string claimType) =>
			@this.TryGetClaimValue(claimType, out var result) ? result : throw new System.Exception("Claim type not found: " + claimType);
	}
}
