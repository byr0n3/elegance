using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// An authenticatable model.
	/// </summary>
	[PublicAPI]
	public interface IAuthenticatable<TAuthenticatable>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
	{
		/// <summary>
		/// An unique identifier for the authenticating entity.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The hashed password (including the salt) to use for authentication.
		/// </summary>
		public byte[] Password { get; }

		/// <summary>
		/// A random value used to detect changes to the entity's credentials and/or security details.
		/// </summary>
		public string? SecurityStamp { get; }

		/// <summary>
		/// The amount of times authentication has failed for this authenticatable entity.
		/// </summary>
		/// <remarks>This value should be reset to <c>0</c> whenever the entity successfully authenticated.</remarks>
		public int AccessFailedCount { get; }

		/// <summary>
		/// Gets the date and time, as an offset from Coordinated Universal Time (UTC), at which the account lockout for this instance ends.
		/// </summary>
		public System.DateTimeOffset? AccessLockoutEnd { get; }

		/// <summary>
		/// Creates an expression to find an authenticatable entity by its user identifier.
		/// </summary>
		/// <param name="user">The user identifier.</param>
		/// <returns>An expression that can be used to filter a queryable collection of entities.</returns>
		public static abstract Expression<System.Func<TAuthenticatable, bool>> FindAuthenticatable(string user);
	}
}
