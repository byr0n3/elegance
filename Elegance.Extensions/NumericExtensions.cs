using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Elegance.Extensions
{
	public static class NumericExtensions
	{
		extension<T>(T @this) where T : INumber<T>
		{
			[PublicAPI]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public string Str([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null,
							  IFormatProvider? provider = null) =>
				@this.ToString(format, provider ?? NumberFormatInfo.InvariantInfo);
		}
	}
}
