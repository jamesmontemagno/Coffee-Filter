using System;

using Foundation;

namespace CoffeeFilter.iOS
{
	public static class LocalizationHelper
	{
		public static string LocalizedString (this string key, string comment)
		{
			return NSBundle.MainBundle.LocalizedString (key, comment);
		}
	}
}

