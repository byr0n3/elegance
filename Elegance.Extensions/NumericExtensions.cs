using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Elegance.Extensions
{
	[PublicAPI]
	public static class NumericExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Str<T>(this T @this,
									[StringSyntax(StringSyntaxAttribute.NumericFormat)]
									string? format = null)
			where T : INumber<T> =>
			@this.ToString(format, NumberFormatInfo.InvariantInfo);
	}
}
