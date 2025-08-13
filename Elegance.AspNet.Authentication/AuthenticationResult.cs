using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Represents the possible results of an authentication attempt.
	/// </summary>
	[PublicAPI]
	public enum AuthenticationResult
	{
		/// <summary>
		/// Represents an unexpected error that occurred during the authentication process.
		/// </summary>
		UnknownError,

		/// <summary>
		/// Indicates that the provided credentials are invalid.
		/// </summary>
		InvalidCredentials,

		/// <summary>
		/// Indicates that the given credentials were correct, but two-factor authentication is required to complete the login process.
		/// </summary>
		TwoFactorRequired,

		/// <summary>
		/// Indicates that the authentication attempt was successful.
		/// </summary>
		Success,
	}
}
