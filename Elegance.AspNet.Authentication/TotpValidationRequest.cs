using System.Runtime.InteropServices;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Represents a request to validate a Time-based One-Time Password (TOTP).
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly ref struct TotpValidationRequest
	{
		/// <summary>
		/// Gets the Unix time (seconds since epoch) at which the TOTP code was generated.
		/// </summary>
		public readonly long Timestamp;

		/// <summary>
		/// Gets the user-provided Time-based One-Time Password (TOTP) code as a read-only span of characters.
		/// </summary>
		public readonly System.ReadOnlySpan<char> Totp;

		internal TotpValidationRequest(System.DateTimeOffset timestamp, System.ReadOnlySpan<char> totp)
		{
			this.Timestamp = (timestamp.Ticks - System.DateTimeOffset.UnixEpoch.Ticks) / System.TimeSpan.TicksPerSecond;
			this.Totp = totp;
		}
	}
}
