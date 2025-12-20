using System;
using System.Runtime.CompilerServices;

namespace Elegance.AspNet.Authentication.Internal
{
	internal static class Base32
	{
		// Reference: https://stackoverflow.com/a/7135008
		public static string ToUrlSafeString(scoped ReadOnlySpan<byte> src)
		{
			var maxLength = (int)float.Ceiling(src.Length / 5f) * 8;

			Span<char> dst = stackalloc char[maxLength];

			byte next = 0;
			byte remaining = 5;
			var written = 0;

			foreach (var b in src)
			{
				next = (byte)(next | (b >> (8 - remaining)));
				dst[written++] = Base32.ValueToChar(next);

				if (remaining < 4)
				{
					next = (byte)((b >> (3 - remaining)) & 31);
					dst[written++] = Base32.ValueToChar(next);
					remaining += 5;
				}

				remaining -= 3;
				next = (byte)((b << remaining) & 31);
			}

			return new string(dst.Slice(0, written));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static char ValueToChar(byte b) =>
			b switch
			{
				< 26 => (char)(b + 65),
				< 32 => (char)(b + 24),

				_ => throw new ArgumentException("Byte is not a Base32 value.", nameof(b))
			};
	}
}
