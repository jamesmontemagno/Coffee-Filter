using NUnit.Framework;
using System;
using Xamarin.UITest;
using CoffeeFilter.UITests.Shared;

namespace CoffeeFilter.UITests
{

	public enum Platform
	{
		Android,
		iOS
	}

	[TestFixture ()]
	public class CoffeeFilterTests
	{

		IApp app;


		public string PathToAPK { get; private set; }

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			PathToAPK = "../../../CoffeeFilter.Android/com.refractored.coffeefilter.apk";
		}

		[TestCase(Platform.Android)]
		public void NoConnectionDisplayError (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.NoConnection);


			var alerts = app.WaitForElement(a => a.Marked("message"));
			app.Screenshot ("Error Message Visible");

			Assert.AreEqual (alerts.Length, 1, "No messages visible");
			Assert.AreEqual (alerts [0].Text, "No Network Connection Available.");
		}

		[TestCase(Platform.Android)]
		public void ParseErrorDisplayError (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.ParseError);
			var alerts = app.WaitForElement(a => a.Marked("message"));
			app.Screenshot ("Parse Message Visible");

			Assert.AreEqual (alerts.Length, 1, "No messages visible");
			Assert.AreEqual (alerts [0].Text, "Unable to get coffee locations.");
		}

		[TestCase(Platform.Android)]
		public void OpenCoffeeDisplayMap (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.OpenCoffee);
			app.WaitForElement (a => a.Marked ("map"));
			app.Screenshot ("Map Visible");
			var top = app.Query (a => a.Marked ("top"));
			var bottom = app.Query (a => a.Marked ("bottom"));
			var rating = app.Query (a => a.Marked ("rating"));

			Assert.AreEqual (top [0].Text, ".013 mi.");
			Assert.AreEqual (bottom [0].Text, "Espresso Vivace");
			Assert.AreEqual (rating [0].Text, "4.5");
		}

		[TestCase(Platform.Android)]
		public void OpenCoffeeNoRating (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.OpenCoffee);
			app.WaitForElement (a => a.Marked ("map"));
			app.Screenshot ("Map Visible");
			var pager = app.Query(a => a.Marked("pager"));
			var rect = pager[0].Rect;
			app.DragCoordinates(rect.Width-5, rect.Y+5, 0, rect.Y+5);
			app.Screenshot ("Second Coffee Location");
			app.DragCoordinates(rect.Width-5, rect.Y+5, 0, rect.Y+5);
			app.Screenshot ("Third Coffee Location");
			var stars = app.Query(a => a.Marked("star"));
			app.Screenshot ("No stars visible");

			Assert.AreEqual (stars.Length, 0, "Stars are visible");
		}

		[TestCase(Platform.Android)]
		public void ClosedCoffeeDisplayError (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.ClosedCoffee);
			var alerts = app.WaitForElement(a => a.Marked("message"));
			app.Screenshot ("Parse Message Visible");

			Assert.AreEqual (alerts.Length, 1, "No messages visible");
			Assert.AreEqual (alerts [0].Text, "There are no coffee shops nearby that are open. Try tomorrow. :(");
		}

		[TestCase(Platform.Android)]
		public void NoLocationsDisplayError (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.NoLocations);
			var alerts = app.WaitForElement(a => a.Marked("message"));
			app.Screenshot ("Parse Message Visible");

			Assert.AreEqual (alerts.Length, 1, "No messages visible");
			Assert.AreEqual (alerts [0].Text, "There are no coffee shops nearby that are open. Try tomorrow. :(");
		}

		[TestCase(Platform.Android)]
		public void UserMovedDisplayCoffee (Platform platform)
		{
			ConfigureTest (platform, UITestsHelpers.TestType.UserMoved);
			var alerts = app.WaitForElement(a => a.Marked("message"));
			app.Screenshot ("Parse Message Visible");

			app.Tap (a => a.Marked ("refresh"));
			app.Screenshot ("Refresh tapped");
			var results = app.WaitForElement (a => a.Marked ("map"));
			app.Screenshot ("Map Visible");
			Assert.AreEqual (results.Length, 1);
		}

		void ConfigureTest(Platform platform, UITestsHelpers.TestType testType)
		{
			switch(platform){
			case Platform.Android:
				ConfigureAndroid ();
				app.Tap(a => a.Marked(testType.ToFriendlyString()));
				break;
			}
		}

		void ConfigureAndroid()
		{
			if (TestEnvironment.Platform == TestPlatform.Local) {
				app = ConfigureApp
					.Android
					.ApkFile (PathToAPK)
					.StartApp ();
			} else {

				app = ConfigureApp
					.Android
					.StartApp ();
			}
		}
	}
}

