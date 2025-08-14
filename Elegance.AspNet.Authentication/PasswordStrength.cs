using System.Buffers;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Utility functions for validating password strength.
	/// </summary>
	[PublicAPI]
	public static class PasswordStrength
	{
		public const int MinLength = 8;

		private static readonly SearchValues<char> capitals =
			SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

		private static readonly SearchValues<char> numbers =
			SearchValues.Create("0123456789");

		private static readonly SearchValues<char> specials =
			SearchValues.Create("!@#$%^&*()-_=+[{]};:'\"\\|,<.>/?§±`~");

		/// <summary>
		/// Validates the strength of a given password.
		/// </summary>
		/// <param name="password">The password to validate. Must contain at least one uppercase letter, one number, and one special character.</param>
		/// <returns>True if the password meets the minimum strength requirements; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ValidateStrength(scoped System.ReadOnlySpan<char> password) =>
			// Minimum length
			(password.Length >= PasswordStrength.MinLength) &&
			// At least one capital character
			PasswordStrength.Has(password, PasswordStrength.capitals) &&
			// At least one number
			PasswordStrength.Has(password, PasswordStrength.numbers) &&
			// At least one special character
			PasswordStrength.Has(password, PasswordStrength.specials);

		/// <summary>
		/// Checks if the specified span contains any characters from the given search values.
		/// </summary>
		/// <param name="value">The span to search within.</param>
		/// <param name="search">The search values containing characters to look for.</param>
		/// <returns>True if any character from 'search' is found in 'value'; otherwise, false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Has(scoped System.ReadOnlySpan<char> value, SearchValues<char> search) =>
			System.MemoryExtensions.IndexOfAny(value, search) != -1;
	}
}
