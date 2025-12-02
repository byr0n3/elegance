using System.Diagnostics;
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

		private readonly TotpService totp;
		private readonly IMemoryCache cache;
		private readonly System.TimeProvider clock;
		private readonly AuthenticationOptions options;
		private readonly System.IServiceProvider services;
		private readonly IDbContextFactory<TDbContext> dbFactory;
		private readonly IClaimsProvider<TAuthenticatable> claimsProvider;
		private readonly ILogger<AuthenticationService<TAuthenticatable, TDbContext>> logger;

		public AuthenticationService(System.IServiceProvider services)
		{
			this.services = services;

			this.totp = services.GetRequiredService<TotpService>();
			this.cache = services.GetRequiredService<IMemoryCache>();
			this.clock = services.GetRequiredService<System.TimeProvider>();
			this.dbFactory = services.GetRequiredService<IDbContextFactory<TDbContext>>();
			this.options = services.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
			this.claimsProvider = services.GetRequiredService<IClaimsProvider<TAuthenticatable>>();
			this.logger = services.GetRequiredService<ILogger<AuthenticationService<TAuthenticatable, TDbContext>>>();
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
		public async ValueTask<AuthenticationResult> AuthenticateAsync(HttpContext context,
																	   string user,
																	   string password,
																	   bool persistent)
		{
			var now = this.clock.GetUtcNow();

			var db = await this.dbFactory.CreateDbContextAsync();

			TAuthenticatable? authenticatable;

			await using (db)
			{
				var set = db.Set<TAuthenticatable>();
				var where = TAuthenticatable.FindAuthenticatable(user, this.services);

				authenticatable = await set.Where((a) => (a.AccessLockoutEnd == null) || (a.AccessLockoutEnd <= now))
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
					// Dont reuse the `now` variable; a little bit of time can have passed since first assigning the `now` variable.
					await queryable.ExecuteUpdateAsync(ResetAccessFailedCount(this.clock.GetUtcNow()));

					// Update the `last sign in timestamp` on the previously queried entity,
					// in case this value gets used when generating claims.
					authenticatable.LastSignInTimestamp = now;
				}
				// The entered password does not match;
				else
				{
					// Subtract 1 from the max attempt count so we lock at the exact moment we reached the threshold.
					var locked = authenticatable.AccessFailedCount >= (this.options.MaxAuthenticationAttempts - 1);

					// The user failed to authenticate too many times; lock them out.
					if (locked)
					{
						await queryable.ExecuteUpdateAsync(LockoutAuthenticatable(authenticatable.AccessFailedCount));
					}
					// Increment the authenticatable's access failed count.
					else
					{
						await queryable.ExecuteUpdateAsync(IncrementAccessFailedCount);
					}

					return locked ? AuthenticationResult.AccountLockedOut : AuthenticationResult.InvalidCredentials;
				}
			}

			if (this.options.EnableMfa && authenticatable.HasMfaEnabled)
			{
				// Store the authenticatable entity in the cache for 1 hour.
				// The cached value will be retrieved once the user has entered their TOTP code.
				this.cache.Set(context.Connection.Id, authenticatable, System.TimeSpan.FromHours(1));

				return AuthenticationResult.MfaRequired;
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

			static System.Action<UpdateSettersBuilder<TAuthenticatable>> ResetAccessFailedCount(System.DateTimeOffset? now) =>
				(builder) => builder.SetProperty(static (a) => a.AccessFailedCount, 0)
									.SetProperty(static (a) => a.AccessLockoutEnd, (System.DateTimeOffset?)null)
									.SetProperty(static (a) => a.LastSignInTimestamp, now);
		}

		/// <summary>
		/// Attempts to authenticate a user using the provided TOTP (Time-based One-Time Password) code.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <param name="code">The TOTP code entered by the user.</param>
		/// <param name="persistent">Indicates whether the authentication state/cookie should be persistent.</param>
		/// <returns>
		/// <para>
		/// A ValueTask that represents the asynchronous operation.
		/// The task result contains a boolean indicating the success or failure of the authentication attempt.
		/// </para>
		/// <para>
		/// This function assumes that the authenticatable entity has tried to authenticate using <see cref="AuthenticateAsync"/> before.
		/// </para>
		/// </returns>
		public async ValueTask<bool> AuthenticateTotpAsync(HttpContext context, string code, bool persistent)
		{
			var request = this.totp.StartValidationRequest(code);

			if (!this.cache.TryGetValue<TAuthenticatable>(context.Connection.Id, out var authenticatable))
			{
				this.logger.LogWarning("[Request:{Id}] Cache miss", context.Connection.Id);
				return false;
			}

			Debug.Assert(authenticatable is not null);

			if (!this.totp.VerifyTotpRequest(request))
			{
				return false;
			}

			this.cache.Remove(context.Connection.Id);

			await this.SignInAsync(context, authenticatable, persistent);

			return true;
		}

		/// <summary>
		/// Signs in an authenticated user asynchronously.
		/// </summary>
		/// <param name="context">The current HTTP context of this request.</param>
		/// <param name="user">The authenticated user to sign in.</param>
		/// <param name="persistent">Indicates whether the authentication state/cookie should be persistent.</param>
		/// <returns>A task that represents the asynchronous sign in operation.</returns>
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
