using System.Collections.Generic;
using System.Security.Claims;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// An authenticatable model.
	/// </summary>
	/// <typeparam name="TAuthenticatable">Reference to self.</typeparam>
	public interface IAuthenticatable<out TAuthenticatable>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
	{
		/// <summary>
		/// Unique identifier.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The hashed password (including the salt) to use for authentication.
		/// </summary>
		public byte[] Password { get; }

		public static abstract TAuthenticatable CreateFromClaims(IEnumerable<Claim> claims);
	}
}
