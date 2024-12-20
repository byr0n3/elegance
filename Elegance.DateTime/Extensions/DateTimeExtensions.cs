using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Elegance.Utilities;
using JetBrains.Annotations;

namespace Elegance.DateTime.Extensions
{
	[PublicAPI]
	public static class DateTimeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.DateOnly ToDateOnly(this System.DateTime @this) =>
			System.DateOnly.FromDateTime(@this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.DateTime ToAmsterdam(this System.DateTime @this) =>
			(@this.Kind is System.DateTimeKind.Unspecified)
				? System.TimeZoneInfo.ConvertTimeFromUtc(@this, TimeZone.Amsterdam)
				: System.TimeZoneInfo.ConvertTime(@this, TimeZone.Amsterdam);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Str(this System.DateTime @this,
								 [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
								 string? format = null) =>
			@this.ToString(format, Culture.NL);
	}
}
