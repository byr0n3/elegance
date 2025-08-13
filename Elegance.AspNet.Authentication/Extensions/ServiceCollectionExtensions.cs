using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Elegance.AspNet.Authentication.Extensions
{
	[PublicAPI]
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds authentication services to the service collection.
		/// </summary>
		/// <param name="services">The IServiceCollection to add the services to.</param>
		/// <param name="configureCookie">An optional Action delegate to configure cookie authentication options.</param>
		/// <param name="configureAuthentication">An optional Action delegate to configure general authentication options.</param>
		/// <typeparam name="TAuthenticatable">The type that implements IAuthenticatable interface for the user entity.</typeparam>
		/// <typeparam name="TDbContext">The type of the DbContext used for data access.</typeparam>
		/// <typeparam name="TClaimsProvider">The type of the claims provider that implements IClaimsProvider interface.</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddAuth<TAuthenticatable, TDbContext, TClaimsProvider>(this IServiceCollection services,
																				  System.Action<CookieAuthenticationOptions>?
																					  configureCookie = null,
																				  System.Action<AuthenticationOptions>?
																					  configureAuthentication = null)
			where TAuthenticatable : class, IAuthenticatable<TAuthenticatable>
			where TDbContext : DbContext
			where TClaimsProvider : class, IClaimsProvider<TAuthenticatable>
		{
			services.AddCascadingAuthenticationState();

			var builder = services.AddAuthentication(AuthenticationService<TAuthenticatable, TDbContext>.Scheme);

			if (configureCookie is not null)
			{
				builder.AddCookie(configureCookie);
			}
			else
			{
				builder.AddCookie();
			}

			if (configureAuthentication is not null)
			{
				services.Configure(configureAuthentication);
			}
			else
			{
				services.AddOptions<AuthenticationOptions>();
			}

			services.AddSingleton<IClaimsProvider<TAuthenticatable>, TClaimsProvider>()
					.AddSingleton<AuthenticationService<TAuthenticatable, TDbContext>>();
		}
	}
}
