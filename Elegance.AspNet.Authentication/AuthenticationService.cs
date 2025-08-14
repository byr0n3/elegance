using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// The main authentication service, handling authentication and querying authentication state.
	/// </summary>
	/// <typeparam name="TAuthenticatable">The type of the authenticatable model.</typeparam>
	/// <typeparam name="TDbContext"></typeparam>
	[PublicAPI]
	public sealed class AuthenticationService<TAuthenticatable, TDbContext>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
		where TDbContext : DbContext
	{
		public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;

		private readonly AuthenticationOptions options;
		private readonly IDbContextFactory<TDbContext> dbFactory;
		private readonly IClaimsProvider<TAuthenticatable> claimsProvider;

		public AuthenticationService(IOptions<AuthenticationOptions> options,
									 IDbContextFactory<TDbContext> dbFactory,
									 IClaimsProvider<TAuthenticatable> claimsProvider)
		{
			this.dbFactory = dbFactory;
			this.options = options.Value;
			this.claimsProvider = claimsProvider;
		}

		/// <summary>
		/// Attempts to sign in a user using the given credentials.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <param name="user">The username or identifier of the user to authenticate.</param>
		/// <param name="password">The password for the user to authenticate.</param>
		/// <param name="persistent">Indicates whether the authentication state/cookie should be persistent.</param>
		/// <returns>
		/// A task that represents the asynchronous operation.
		/// The task result contains an AuthenticationResult value indicating the success or failure of the authentication attempt.
		/// </returns>
		public async ValueTask<AuthenticationResult> AuthenticateAsync(HttpContext context, string user, string password, bool persistent)
		{
			var db = await this.dbFactory.CreateDbContextAsync();

			TAuthenticatable? authenticatable;

			await using (db)
			{
				var set = db.Set<TAuthenticatable>();
				var where = TAuthenticatable.FindAuthenticatable(user);

				authenticatable = await set.Where(static (a) => a.AccessLockoutEnd <= System.DateTimeOffset.UtcNow)
										   .Where(where)
										   .FirstOrDefaultAsync();

				if (authenticatable is null)
				{
					return AuthenticationResult.InvalidCredentials;
				}

				var queryable = set.Where((a) => a.Id == authenticatable.Id);

				// If the entered password matches the hash in the database, authenticating was successful.
				// We should reset the authenticatable their access failed count.
				if (Hashing.Verify(authenticatable.Password, password))
				{
					await queryable.ExecuteUpdateAsync(ResetAccessFailedCount);
				}
				// The entered password does not match;
				else
				{
					// The user failed to authenticate too many times; lock them out.
					if (authenticatable.AccessFailedCount >= this.options.MaxAuthenticationAttempts)
					{
						await queryable.ExecuteUpdateAsync(LockoutAuthenticatable(authenticatable.AccessFailedCount));
					}
					// Increment the authenticatable's access failed count.
					else
					{
						await queryable.ExecuteUpdateAsync(IncrementAccessFailedCount);
					}

					return AuthenticationResult.InvalidCredentials;
				}
			}

			// @todo Check 2FA
			if (false)
			{
				return AuthenticationResult.TwoFactorRequired;
			}

			await this.SignInAsync(context, authenticatable, persistent);

			return AuthenticationResult.Success;

			System.Action<UpdateSettersBuilder<TAuthenticatable>> LockoutAuthenticatable(int accessFailedCount)
			{
				// If incremental lockout duration is enabled,
				// the length of the lockout is dependent on how many times authentication has failed.
				// Otherwise, count should always end up at 1, so we make it 0 here.
				var count = this.options.IncrementalLockoutDuration ? (this.options.MaxAuthenticationAttempts / accessFailedCount) : 0;

				// We need to add 1 to the count, as `count` starts at 0.
				count++;

				var value = System.DateTimeOffset.UtcNow + (count * this.options.AuthenticationLockoutDuration);

				return (builder) => builder.SetProperty(static (a) => a.AccessLockoutEnd, value);
			}

			static void IncrementAccessFailedCount(UpdateSettersBuilder<TAuthenticatable> builder) =>
				builder.SetProperty(static (a) => a.AccessFailedCount, static (a) => a.AccessFailedCount + 1);

			static void ResetAccessFailedCount(UpdateSettersBuilder<TAuthenticatable> builder) =>
				builder.SetProperty(static (a) => a.AccessFailedCount, 0);
		}

		/// <summary>
		/// Signs in an authenticated user asynchronously.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <param name="user">The authenticated user to log in.</param>
		/// <param name="persistent">Indicates whether the authentication state/cookie should be persistent.</param>
		/// <returns>A task that represents the asynchronous login operation.</returns>
		public async Task SignInAsync(HttpContext context, TAuthenticatable user, bool persistent)
		{
			var now = System.DateTimeOffset.UtcNow;

			var auth = new AuthenticationProperties
			{
				AllowRefresh = true,
				IsPersistent = persistent,
				ExpiresUtc = persistent ? System.DateTimeOffset.MaxValue : now.AddHours(1),
				IssuedUtc = now,
			};

			var identity = new ClaimsIdentity(null, AuthenticationService<TAuthenticatable, TDbContext>.Scheme);

			await foreach (var claim in this.claimsProvider.GetClaimsAsync(user, context.RequestAborted))
			{
				identity.AddClaim(claim);
			}

			await context.SignInAsync(AuthenticationService<TAuthenticatable, TDbContext>.Scheme, new ClaimsPrincipal(identity), auth);
		}

		/// <summary>
		/// Signs out the user by invalidating their authentication state.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <returns>A task that represents the asynchronous operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Task SignOutAsync(HttpContext context) =>
			context.SignOutAsync(AuthenticationService<TAuthenticatable, TDbContext>.Scheme, null);
	}
}
