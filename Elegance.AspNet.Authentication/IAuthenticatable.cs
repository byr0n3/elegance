namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// An authenticatable model.
	/// </summary>
	public interface IAuthenticatable
	{
		/// <summary>
		/// Unique identifier.
		/// </summary>
		public int Id { get; }

		/// <summary>
		/// The hashed password (including the salt) to use for authentication.
		/// </summary>
		public byte[] Password { get; }
	}
}
