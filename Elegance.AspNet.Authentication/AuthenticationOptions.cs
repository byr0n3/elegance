using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Elegance.AspNet.Authentication
{
	[PublicAPI]
	public sealed class AuthenticationOptions : IOptions<AuthenticationOptions>
	{
		/// <summary>
		/// Gets or sets the maximum number of allowed authentication attempts before locking out an account.
		/// </summary>
		public int MaxAuthenticationAttempts { get; set; } = 5;

		/// <summary>
		/// Gets or sets the duration for which an account is locked out after exceeding the maximum number of authentication attempts.
		/// </summary>
		public System.TimeSpan AuthenticationLockoutDuration { get; set; } = System.TimeSpan.FromHours(1);

		/// <summary>
		/// Gets or sets a value indicating whether the lockout duration should increase every time it gets triggered.
		/// </summary>
		/// <example>
		/// <p>Assuming <see cref="MaxAuthenticationAttempts"/> is 5 and <see cref="AuthenticationLockoutDuration"/> is 1 hour:</p>
		/// <list type="bullet">
		/// <item>The first time an account gets locked out, it will be locked for 1 hour</item>
		/// <item>If the account fails to log in again and triggers the lockout, the account will be locked for 2 hours instead of 1.</item>
		/// </list>
		/// </example>
		public bool IncrementalLockoutDuration { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether 'multifactor authentication' (MFA) is enabled.
		/// </summary>
		public bool EnableMfa { get; set; } = true;

		AuthenticationOptions IOptions<AuthenticationOptions>.Value =>
			this;
	}
}
