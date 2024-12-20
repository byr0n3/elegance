using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Elegance.Utilities;
using JetBrains.Annotations;

namespace Elegance.DateTime.Extensions
{
	[PublicAPI]
	public static class DateOnlyExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Str(this System.DateOnly @this,
								 [StringSyntax(StringSyntaxAttribute.DateOnlyFormat)]
								 string? format = null) =>
			@this.ToString(format, Culture.NL);
	}
}
