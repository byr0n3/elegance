using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// The main authentication service, handling authentication and querying authentication state.
	/// </summary>
	/// <typeparam name="TAuthenticatable">The type of the authenticatable model.</typeparam>
	public sealed class AuthenticationService<TAuthenticatable>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
	{
		internal const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

		private readonly IClaimsProvider<TAuthenticatable> claimsProvider;

		public AuthenticationService(IClaimsProvider<TAuthenticatable> claimsProvider)
		{
			this.claimsProvider = claimsProvider;
		}

		/// <summary>
		/// Get the currently authenticated user.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <returns>The currently authenticated user if authenticated, <see langword="null"/> otherwise.</returns>
		public TAuthenticatable? GetUser(HttpContext context)
		{
			var user = context.User;

			return user.Identity?.IsAuthenticated == true ? TAuthenticatable.CreateFromClaims(user.Claims) : null;
		}

		/// <summary>
		/// Signs in as the given <paramref name="user"/>.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <param name="user">The user to sign in as.</param>
		/// <param name="persistent">Should the authentication state/cookie be persistent?</param>
		/// <param name="token">Cancellation token.</param>
		public async Task LoginAsync(HttpContext context, TAuthenticatable user, bool persistent, CancellationToken token = default)
		{
			var now = System.DateTimeOffset.UtcNow;

			var auth = new AuthenticationProperties
			{
				AllowRefresh = true,
				IsPersistent = persistent,
				ExpiresUtc = persistent ? System.DateTimeOffset.MaxValue : now.AddHours(1),
				IssuedUtc = now,
			};

			var identity = new ClaimsIdentity(null, AuthenticationService<TAuthenticatable>.Scheme);

			await foreach (var claim in this.claimsProvider.GetClaimsAsync(user, token))
			{
				identity.AddClaim(claim);
			}

			await context.SignInAsync(AuthenticationService<TAuthenticatable>.Scheme, new ClaimsPrincipal(identity), auth);
		}

		/// <summary>
		/// Removes the current authentication state from the authenticated user.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <exception cref="System.NullReferenceException">There's no HTTP context available.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task LogoutAsync(HttpContext context) =>
			context.SignOutAsync(AuthenticationService<TAuthenticatable>.Scheme, null);
	}
}
