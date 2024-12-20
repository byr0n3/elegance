using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Elegance.AspNet
{
	public sealed class UintRouteConstraint : IRouteConstraint, IParameterLiteralNodeMatchingPolicy
	{
		public bool Match(HttpContext? ctx, IRouter? _, string routeKey, RouteValueDictionary values, RouteDirection __)
		{
			// Parameter doesn't exist.
			if (!values.TryGetValue(routeKey, out var @object))
			{
				return false;
			}

			// Parameter is already mapped.
			if (@object is uint)
			{
				return true;
			}

			var @string = System.Convert.ToString(@object, CultureInfo.InvariantCulture);

			var valid = (@string is not null) && this.MatchesLiteral(string.Empty, @string);

			// `Minimal API` routing doesn't want us to parse the value, but the Blazor router does.
			if (!UintRouteConstraint.IsApiRequest(ctx) &&
				uint.TryParse(@string, NumberStyles.None, NumberFormatInfo.InvariantInfo, out var @uint))
			{
				values[routeKey] = @uint;
			}

			return valid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MatchesLiteral(string __, string literal) =>
			uint.TryParse(literal, NumberStyles.None, NumberFormatInfo.InvariantInfo, out _);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsApiRequest(HttpContext? ctx) =>
			ctx?.Request.Path.StartsWithSegments("/api", System.StringComparison.Ordinal) == true;
	}
}
