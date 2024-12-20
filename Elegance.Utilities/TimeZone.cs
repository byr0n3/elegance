using JetBrains.Annotations;

namespace Elegance.Utilities
{
	[PublicAPI]
	public static class TimeZone
	{
		public static readonly System.TimeZoneInfo Amsterdam =
			System.TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
	}
}
