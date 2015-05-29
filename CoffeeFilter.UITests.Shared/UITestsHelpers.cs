using System;
using System.ComponentModel;

namespace CoffeeFilter.UITests.Shared
{
	public static class UITestsHelpers
	{
		public static string XTCApiKey {
			get;
		} = "024b0d715a7e9c22388450cf0069cb19";

		public static TestType SelectedTest { get; set; }
		public enum TestType
		{
			OpenCoffee = 0,
			NoConnection,
			ParseError,
			ClosedCoffee,
			NoLocations,
			UserMoved
		}
	}

	public static class TestTypeExtensions
	{
		public static string ToFriendlyString(this UITestsHelpers.TestType me)
		{
			switch(me)
			{
			case UITestsHelpers.TestType.NoConnection:
				return "No Internet Connection";
			case UITestsHelpers.TestType.ParseError:
				return "Parse Error";
			case UITestsHelpers.TestType.OpenCoffee:
				return "Open Coffee Locations";
			case UITestsHelpers.TestType.ClosedCoffee:
				return "Closed Coffee Locations";
			case UITestsHelpers.TestType.NoLocations:
				return "No Locations Around";
			case UITestsHelpers.TestType.UserMoved:
				return "User Moved and Refreshed";
			}
			return string.Empty;
		}
	}

}

