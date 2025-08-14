namespace Elegance.AspNet.Authentication.Internal
{
	/// <summary>
	/// Provides constant values used within the authentication process.
	/// </summary>
	internal static class Constants
	{
		/// <summary>
		/// Represents the claim type used to identify a user by their ID.
		/// </summary>
		public const string IdClaimType = "id";

		/// <summary>
		/// Represents the claim type used to identify a user by their security stamp.
		/// </summary>
		public const string SecurityStampClaimType = "ss";
	}
}
