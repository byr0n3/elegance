using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace Elegance.AspNet.Authentication.Extensions
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers the required authentication services for users of the type <typeparamref name="TAuthenticatable"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to register the services to.</param>
		/// <param name="configure">Callback to configure authentication options.</param>
		/// <typeparam name="TAuthenticatable">The type of the authenticatable model.</typeparam>
		/// <typeparam name="TClaimsProvider">The type of the claims provider to use.</typeparam>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddAuth<TAuthenticatable, TClaimsProvider>(this IServiceCollection services,
																	  System.Action<CookieAuthenticationOptions>? configure)
			where TAuthenticatable : class, IAuthenticatable
			where TClaimsProvider : class, IClaimsProvider<TAuthenticatable>
		{
			services.AddCascadingAuthenticationState();

			var builder = services.AddAuthentication(AuthenticationService<TAuthenticatable>.Scheme);

			if (configure is not null)
			{
				builder.AddCookie(configure);
			}
			else
			{
				builder.AddCookie();
			}

			services.AddSingleton<IClaimsProvider<TAuthenticatable>, TClaimsProvider>()
					.AddSingleton<AuthenticationService<TAuthenticatable>>();
		}
	}
}
