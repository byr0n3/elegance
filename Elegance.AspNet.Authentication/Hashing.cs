using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Utility functions for hashing and validation.
	/// </summary>
	[PublicAPI]
	public static class Hashing
	{
		/// <summary>
		/// The number of bytes a hash will have.
		/// </summary>
		public const int HashSize = Hashing.saltLength + Hashing.subKeyLength;

		private const int saltLength = 16;
		private const int subKeyLength = 32;

		private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

		/// <summary>
		/// Takes the given <paramref name="value"/> and returns a SHA256 hash representing the given <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to generate a hash for.</param>
		/// <returns>The generated SHA256 representation.</returns>
		public static byte[] Hash(scoped ReadOnlySpan<char> value)
		{
			var hash = new byte[Hashing.HashSize];

			Hashing.Hash(value, hash);

			return hash;
		}

		/// <summary>
		/// Takes the given <paramref name="value"/> and write the SHA256 hash bytes into <paramref name="dst"/>.
		/// </summary>
		/// <param name="value">The value to generate a hash for.</param>
		/// <param name="dst">The target location to write the hash to.</param>
		/// <returns>The number of bytes written to <paramref name="dst"/></returns>
		public static int Hash(scoped ReadOnlySpan<char> value, scoped Span<byte> dst)
		{
			Debug.Assert(dst.Length >= Hashing.HashSize);

			// Generate salt
			var salt = dst.Slice(0, Hashing.saltLength);
			Hashing.rng.GetBytes(salt);

			// Generate sub key
			var subKey = dst.Slice(Hashing.saltLength);
			Hashing.GenerateSubKey(value, salt, subKey);

			return Hashing.HashSize;
		}

		/// <summary>
		/// Compares the given SHA256 <paramref name="hash"/> and compares it to the SHA256 representation of the given <paramref name="value"/>.
		/// </summary>
		/// <param name="hash">The SHA256 to compare to.</param>
		/// <param name="value">The value to compare to the <paramref name="hash"/>.</param>
		/// <returns><see langword="true"/> if the SHA256 representation of <paramref name="value"/> is equal to the given <paramref name="hash"/>, <see langword="false"/> otherwise.</returns>
		public static bool Verify(scoped ReadOnlySpan<byte> hash, scoped ReadOnlySpan<char> value)
		{
			// The length of the subkey should be based off of hashed password for compatibility reasons
			var inputSubKeyLength = (hash.Length - Hashing.saltLength);

			// The salt from hashed password
			var salt = hash.Slice(0, Hashing.saltLength);
			// The subkey from hashed password
			var subKey = hash.Slice(Hashing.saltLength);

			// The subkey from input, generated using salt from hashed password
			Span<byte> inputSubKey = stackalloc byte[inputSubKeyLength];
			Hashing.GenerateSubKey(value, salt, inputSubKey);

			return subKey.SequenceEqual(inputSubKey);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void GenerateSubKey(ReadOnlySpan<char> value, ReadOnlySpan<byte> salt, Span<byte> dst) =>
			Rfc2898DeriveBytes.Pbkdf2(value, salt, dst, 100, HashAlgorithmName.SHA256);
	}
}
