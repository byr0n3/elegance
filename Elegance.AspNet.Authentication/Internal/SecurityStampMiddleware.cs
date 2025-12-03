using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elegance.AspNet.Authentication.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Elegance.AspNet.Authentication.Internal
{
	/// <summary>
	/// Middleware for validating security stamps of authenticatable entities.
	/// </summary>
	/// <typeparam name="TAuthenticatable">The type implementing IAuthenticatable.</typeparam>
	/// <typeparam name="TDbContext">The type derived from DbContext.</typeparam>
	internal sealed class SecurityStampMiddleware<TAuthenticatable, TDbContext>
		where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
		where TDbContext : DbContext
	{
		private readonly RequestDelegate next;
		private readonly IDbContextFactory<TDbContext> dbFactory;
		private readonly AuthenticationService<TAuthenticatable, TDbContext> authentication;

		public SecurityStampMiddleware(RequestDelegate next,
									   IDbContextFactory<TDbContext> dbFactory,
									   AuthenticationService<TAuthenticatable, TDbContext> authentication)
		{
			this.next = next;
			this.dbFactory = dbFactory;
			this.authentication = authentication;
		}

		/// <summary>
		/// Invokes the middleware to handle the security stamp validation process for an authenticatable entities.
		/// </summary>
		/// <param name="context">The HTTP context containing information about the request.</param>
		public async Task InvokeAsync(HttpContext context)
		{
			if (context.User.Identity?.IsAuthenticated == true)
			{
				await this.HandleAsync(context);
			}

			await this.next.Invoke(context);
		}

		/// <summary>
		/// Handles the security stamp validation process for an authenticatable entities.
		/// </summary>
		/// <param name="context">The HTTP context containing information about the request.</param>
		private async Task HandleAsync(HttpContext context)
		{
			if (!context.User.TryGetClaimValue<int>(Constants.IdClaimType, out var id) ||
				!context.User.TryGetClaimValue(Constants.SecurityStampClaimType, out var securityStamp) ||
				!await this.IsSecurityStampValidAsync(id, securityStamp, context.RequestAborted))
			{
				await this.authentication.SignOutAsync(context);
			}
		}

		/// <summary>
		/// Asynchronously validates the security stamp of an authenticatable entity.
		/// </summary>
		/// <param name="id">The identifier of the authenticatable entity.</param>
		/// <param name="value">The security stamp value to validate.</param>
		/// <param name="token">The cancellation token that can be used to propagate notification that operations should be canceled.</param>
		/// <returns>A task that represents the asynchronous operation. The task result is true if the security stamp is valid; otherwise, false.</returns>
		private async ValueTask<bool> IsSecurityStampValidAsync(int id, string value, CancellationToken token = default)
		{
			await using (var db = await this.dbFactory.CreateDbContextAsync(token))
			{
				return await db.Set<TAuthenticatable>()
							   .Where((a) => a.Id == id)
							   .AnyAsync((a) => (a.Id == id) && (a.SecurityStamp == value), token);
			}
		}
	}
}
