using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace Elegance.AspNet.Authentication
{
	[PublicAPI]
	public sealed class TotpOptions : IOptions<TotpOptions>
	{
		/// <summary>
		/// Gets or sets the secret key in byte array format used for generating time-based one-time passwords (TOTP).
		/// </summary>
		internal byte[]? SecretKeyBytes { get; private set; }

		/// <summary>
		/// Gets or sets the secret key used for generating time-based one-time passwords (TOTP).
		/// </summary>
		public string? SecretKey
		{
			get => this.SecretKeyBytes is not null ? Encoding.UTF8.GetString(this.SecretKeyBytes) : null;
			set => this.SecretKeyBytes = value is not null ? Encoding.UTF8.GetBytes(value) : null;
		}

		/// <summary>
		/// Gets or sets how long a time-based one-time password (TOTP) should be valid.
		/// </summary>
		public int Step { get; set; } = 30;

		/// <summary>
		/// Gets or sets the size of the time-based one-time password (TOTP) code.
		/// </summary>
		public int Size { get; set; } = 6;

		/// <summary>
		/// Gets or sets the hash algorithm used for generating time-based one-time passwords (TOTP).
		/// </summary>
		public TotpHashAlgorithm HashAlgorithm { get; set; } = TotpHashAlgorithm.SHA1;

		/// <summary>
		/// Gets or sets the lower bound of the acceptable time drift (in intervals defined by <see cref="Step"/>) for validating a one-time password (OTP).
		/// This allows some flexibility in the validation window to account for clock skew.
		/// </summary>
		public int WindowStepLowerOffset { get; set; }

		/// <summary>
		/// Gets or sets the upper bound of the acceptable time drift (in intervals defined by <see cref="Step"/>) for validating a one-time password (OTP).
		/// This allows some flexibility in the validation window to account for clock skew.
		/// </summary>
		public int WindowStepUpperOffset { get; set; }

		/// <summary>
		/// Gets or sets the issuer of the time-based one-time password (TOTP).
		/// This property is used to identify the entity that generates and manages the TOTP.
		/// </summary>
		public string? Issuer { get; set; }

		TotpOptions IOptions<TotpOptions>.Value =>
			this;
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum TotpHashAlgorithm
	{
		SHA1,
		SHA256,
		SHA512,
	}
}
