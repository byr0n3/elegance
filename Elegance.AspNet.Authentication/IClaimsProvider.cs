using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Provides claims for the authenticatable model of type <typeparamref name="TAuthenticatable"/>.
	/// </summary>
	/// <typeparam name="TAuthenticatable">The type of the authenticatable model.</typeparam>
	[PublicAPI]
	public interface IClaimsProvider<in TAuthenticatable>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
	{
		/// <summary>
		/// Get the authentication claims for the authenticating <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user that is currently being authenticated.</param>
		/// <param name="token">Cancellation token.</param>
		/// <returns>The generated claims for the <paramref name="user"/>.</returns>
		/// <remarks>This method returns a <see cref="IAsyncEnumerable{Claim}"/>, so claims can be retrieved from a database if needed.</remarks>
		public IAsyncEnumerable<Claim> GetClaimsAsync(TAuthenticatable user, CancellationToken token);
	}
}
