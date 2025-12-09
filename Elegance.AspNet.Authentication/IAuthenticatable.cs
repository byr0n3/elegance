using System.Linq;
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
		/// Gets the timestamp indicating when the entity last signed in.
		/// </summary>
		/// <remarks>The property setter is required to update the timestamp of an authenticating entity.</remarks>
		public System.DateTimeOffset? LastSignInTimestamp { get; set; }

		/// <summary>
		/// The amount of times authentication has failed for the authenticating entity.
		/// </summary>
		/// <remarks>This value should be reset to <c>0</c> whenever the entity successfully authenticated.</remarks>
		public int AccessFailedCount { get; }

		/// <summary>
		/// Gets the date and time, as an offset from Coordinated Universal Time (UTC), at which the account lockout for this instance ends.
		/// </summary>
		public System.DateTimeOffset? AccessLockoutEnd { get; }

		/// <summary>
		/// Gets a value indicating whether 'multifactor authentication' (MFA) is enabled for the authenticating entity.
		/// </summary>
		public bool HasMfaEnabled { get; }

		/// <summary>
		/// Creates an expression to find an authenticating entity by its user identifier.
		/// </summary>
		/// <param name="user">The user identifier.</param>
		/// <param name="services">Collection of registered services.</param>
		/// <returns>An expression that can be used to filter a queryable collection of entities.</returns>
		public static abstract Expression<System.Func<TAuthenticatable, bool>> FindAuthenticatable(string user,
																								   System.IServiceProvider services);

		public static abstract IQueryable<TAuthenticatable> Include(IQueryable<TAuthenticatable> queryable);
	}
}
