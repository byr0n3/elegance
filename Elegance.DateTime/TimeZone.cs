using JetBrains.Annotations;

namespace Elegance.DateTime
{
	[PublicAPI]
	public static class TimeZone
	{
		public static readonly System.TimeZoneInfo Amsterdam =
			System.TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
	}
}
