using System;
using System.Globalization;

namespace CoffeeFilter.Shared.Helpers
{
	public static class DateTimeUtils
	{
		static readonly DateTime Epoch = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime ParseUnixTime (string text)
		{
			var seconds = double.Parse (text, CultureInfo.InvariantCulture);
			return ParseUnixTime (seconds);
		}

		public static DateTime ParseUnixTime (double seconds)
		{
			return Epoch.AddSeconds(seconds);
		}

		public static double DateTimeToUnixTime (DateTime date)
		{
			TimeSpan diff = date - Epoch;
			return Math.Floor (diff.TotalSeconds);
		}
	}
}

