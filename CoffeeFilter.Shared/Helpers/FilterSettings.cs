using Refractored.Xam.Settings;
using Refractored.Xam.Settings.Abstractions;

namespace CoffeeFilter.Shared.Helpers
{
	/// <summary>
	/// This is the Settings static class that can be used in your Core solution or in any
	/// of your client applications. All settings are laid out the same exact way with getters
	/// and setters. 
	/// </summary>
	public static class FilterSettings
	{
		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		#region Setting Constants

		private const string VersionKey = "version_key";
		private static readonly string VersionDefault = "1.0.0";

		#endregion


		public static string Version
		{
			get
			{
				return AppSettings.GetValueOrDefault(VersionKey, VersionDefault);
			}
			set
			{
				AppSettings.AddOrUpdateValue(VersionKey, value);
			}
		}

	}
}