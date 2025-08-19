using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Elegance.AspNet.Authentication.Internal;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Provides functionality for generating and verifying Time-based One-Time Passwords (TOTP).
	/// </summary>
	[PublicAPI]
	public sealed class TotpService
	{
		private readonly TotpOptions options;
		private readonly System.TimeProvider clock;

		public TotpService(System.TimeProvider clock, IOptions<TotpOptions> options)
		{
			this.clock = clock;
			this.options = options.Value;
		}

		/// <summary>
		/// Generates a URI for a Time-based One-Time Password (TOTP) setup.
		/// </summary>
		/// <param name="user">The username or user identifier to associate with the TOTP.</param>
		/// <returns>A URI string formatted for setting up TOTP on an authenticator app.</returns>
		public string CreateUri(string user)
		{
			Debug.Assert(this.options.SecretKeyBytes is not null);

			var queryParameters = new List<KeyValuePair<string, string?>>
			{
				new("secret", Base32.ToUrlSafeString(this.options.SecretKeyBytes)),
				new("issuer", this.options.Issuer is not null ? System.Uri.EscapeDataString(this.options.Issuer) : null),
				// @todo Hard-coded extension method
				new("algorithm", this.options.HashAlgorithm.ToString()),
				new("digits", this.options.Size.ToString("N0", NumberFormatInfo.InvariantInfo)),
				new("period", this.options.Step.ToString("N0", NumberFormatInfo.InvariantInfo)),
			};

			var builder = new StringBuilder("otpauth://totp/");

			if (!string.IsNullOrWhiteSpace(this.options.Issuer))
			{
				builder.Append(this.options.Issuer)
					   .Append(':');
			}

			builder.Append(System.Uri.EscapeDataString(user));
			builder.Append(QueryString.Create(queryParameters).ToUriComponent());

			var result = builder.ToString();

			return result;
		}

		/// <summary>
		/// Creates a new Time-based One-Time Password (TOTP) validation request.
		/// </summary>
		/// <param name="totp">The user-provided TOTP code as a read-only span of characters.</param>
		/// <returns>A new instance of the <see cref="TotpValidationRequest"/> class initialized with the current UTC time and the provided TOTP code.</returns>
		/// /// <remarks>
		/// It's important to create a validation request right as the OTP-validation request begins.
		/// This ensures that the filled-in TOTP value gets validated for the correct time window.
		/// </remarks>
		public TotpValidationRequest StartValidationRequest(System.ReadOnlySpan<char> totp) =>
			new(this.clock.GetUtcNow(), totp);

		/// <summary>
		/// Verifies the validity of the provided TOTP validation request.
		/// </summary>
		/// <param name="request">The <see cref="TotpValidationRequest"/> to verify.</param>
		/// <returns><see langword="true" /> if the TOTP code in the request is valid; otherwise, <see langword="false" />.</returns>
		public bool VerifyTotpRequest(TotpValidationRequest request)
		{
			System.Span<char> buffer = stackalloc char[this.options.Size];

			var requestStep = request.Timestamp / this.options.Step;

			var min = requestStep - this.options.WindowStepLowerOffset;
			var max = requestStep + this.options.WindowStepLowerOffset;

			// Check the validity of the TOTP code for each time step in the time window.
			// This is done to account for clock skew.
			for (var step = min; step <= max; step++)
			{
				var written = this.GetTotpForStep(buffer, step);

				if (TotpService.Compare(request.Totp, buffer.Slice(0, written)))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Computes the TOTP code for a specific step and writes it to the provided buffer.
		/// </summary>
		/// <param name="buffer">The span of characters where the computed TOTP will be written.</param>
		/// <param name="step">The time step for which the TOTP is being computed.</param>
		/// <returns>The number of characters written in the buffer representing the TOTP code.</returns>
		/// <exception cref="System.ArgumentException">Thrown when an invalid hash algorithm is specified in the options.</exception>
		private int GetTotpForStep(scoped System.Span<char> buffer, long step)
		{
			System.Span<byte> stepBytes = stackalloc byte[sizeof(long)];

			TotpService.GetBytesBigEndian(stepBytes, step);

			var otpStep = this.ComputeHmac(stepBytes);

			var code = (int)otpStep % (int)System.Math.Pow(10, this.options.Size);

			var bufferOffset = PrefixBuffer(buffer, code);

			var done = code.TryFormat(buffer.Slice(bufferOffset), out var written, default, NumberFormatInfo.InvariantInfo);

			Debug.Assert(done);

			return written;

			// Prefixes the buffer with leading zeros to ensure that the buffer is always the configured length.
			// The length of `buffer` should be set to the configured length of the TOTP code.
			static int PrefixBuffer(scoped System.Span<char> buffer, int code)
			{
				var resultStrLength = TotpService.GetStringLength(code);
				var bufferOffset = int.Abs(resultStrLength - buffer.Length);

				for (var i = 0; i < bufferOffset; i++)
				{
					buffer[i] = '0';
				}

				return bufferOffset;
			}
		}

		/// <summary>
		/// Computes the HMAC value for a given input span and generates a TOTP code.
		/// </summary>
		/// <param name="src">The input data as a read-only span of bytes.</param>
		/// <returns>The generated TOTP code as a long integer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when an invalid hash algorithm is specified in the options.</exception>
		private long ComputeHmac(scoped System.ReadOnlySpan<byte> src)
		{
			System.Span<byte> dst = stackalloc byte[GetHmacSize(this.options.HashAlgorithm)];

			Debug.Assert(this.options.SecretKeyBytes is not null);

			var hmac = CreateHmac(this.options.HashAlgorithm, this.options.SecretKeyBytes);

			var done = hmac.TryComputeHash(src, dst, out var written);

			Debug.Assert(done);

			dst = dst.Slice(0, written);

			// Truncate the last 4 bits of the last byte.
			// This is technically the last 4 bits of the FIRST byte as `src` is a big-endian value.
			var hmacOffset = dst[^1] & 0x0f;

			return (dst[hmacOffset] & 0x7f) << 24 |
				   (dst[hmacOffset + 1] & 0xff) << 16 |
				   (dst[hmacOffset + 2] & 0xff) << 8 |
				   (dst[hmacOffset + 3] & 0xff);

			// Suppress the 'weak cryptography' warning; RFC 6238 indicates that SHA1 is fine for TOTP tokens.
			[SuppressMessage("Security", "CA5350")]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static HMAC CreateHmac(TotpHashAlgorithm algorithm, byte[] secretKey) =>
				(algorithm) switch
				{
					TotpHashAlgorithm.SHA1   => new HMACSHA1(secretKey),
					TotpHashAlgorithm.SHA256 => new HMACSHA256(secretKey),
					TotpHashAlgorithm.SHA512 => new HMACSHA512(secretKey),

					_ => throw new System.ArgumentException("Invalid algorithm name: " + algorithm, nameof(algorithm)),
				};

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static int GetHmacSize(TotpHashAlgorithm algorithm) =>
				(algorithm) switch
				{
					TotpHashAlgorithm.SHA1   => HMACSHA1.HashSizeInBytes,
					TotpHashAlgorithm.SHA256 => HMACSHA256.HashSizeInBytes,
					TotpHashAlgorithm.SHA512 => HMACSHA512.HashSizeInBytes,

					_ => throw new System.ArgumentException("Invalid algorithm name: " + algorithm, nameof(algorithm)),
				};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void GetBytesBigEndian(scoped System.Span<byte> dst, long value)
		{
			var done = System.BitConverter.TryWriteBytes(dst, value);

			Debug.Assert(done);

			// If the system is little-endian, reverse the bytes to make the value big-endian.
			if (System.BitConverter.IsLittleEndian)
			{
				System.MemoryExtensions.Reverse(dst);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int GetStringLength(int value) =>
			value switch
			{
				>= 1000000000 => 10,
				>= 100000000  => 9,
				>= 10000000   => 8,
				>= 1000000    => 7,
				>= 100000     => 6,
				>= 10000      => 5,
				>= 1000       => 4,
				>= 100        => 3,
				>= 10         => 2,
				_             => 1,
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Compare(scoped System.ReadOnlySpan<char> lhs, scoped System.ReadOnlySpan<char> rhs) =>
			System.MemoryExtensions.SequenceEqual(lhs, rhs);
	}
}
