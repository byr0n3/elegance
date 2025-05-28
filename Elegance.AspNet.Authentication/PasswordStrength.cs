using System.Buffers;
using System.Runtime.CompilerServices;

namespace Elegance.AspNet.Authentication
{
	/// <summary>
	/// Utility functions for validating password strength.
	/// </summary>
	public static class PasswordStrength
	{
		public const int MinLength = 8;

		private static readonly SearchValues<char> capitals =
			SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

		private static readonly SearchValues<char> numbers =
			SearchValues.Create("0123456789");

		private static readonly SearchValues<char> specials =
			SearchValues.Create("!@#$%^&*()-_=+[{]};:'\"\\|,<.>/?§±`~");

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool Has(scoped System.ReadOnlySpan<char> value, SearchValues<char> search) =>
			System.MemoryExtensions.IndexOfAny(value, search) != -1;
	}
}
