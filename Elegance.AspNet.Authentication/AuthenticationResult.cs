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
		/// Indicates that 'multifactor authentication' (MFA) is required for the user to complete the authentication process.
		/// </summary>
		MfaRequired,

		/// <summary>
		/// Indicates that the account that the currently attempting to sign in has been locked.
		/// </summary>
		AccountLockedOut,

		/// <summary>
		/// Indicates that the authentication attempt was successful.
		/// </summary>
		Success,
	}
}
