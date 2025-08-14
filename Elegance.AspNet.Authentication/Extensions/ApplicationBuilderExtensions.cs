using Elegance.AspNet.Authentication.Internal;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Elegance.AspNet.Authentication.Extensions
{
	[PublicAPI]
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds authentication and authorization middleware to the application pipeline.
		/// </summary>
		/// <param name="app">The <see cref="IApplicationBuilder"/> used to configure the HTTP request pipeline.</param>
		/// <typeparam name="TAuthenticatable">The type of the authenticatable model implementing IAuthenticatable interface.</typeparam>
		/// <typeparam name="TDbContext">The type of the database context derived from DbContext class.</typeparam>
		/// <returns>The same instance of <see cref="IApplicationBuilder"/> with added authentication and authorization middleware.</returns>
		public static IApplicationBuilder UseAuth<TAuthenticatable, TDbContext>(this IApplicationBuilder app)
			where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
			where TDbContext : DbContext
		{
			app.UseMiddleware<SecurityStampMiddleware<TAuthenticatable, TDbContext>>();

			app.UseAuthentication();
			app.UseAuthorization();

			return app;
		}
	}
}
