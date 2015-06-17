using NUnit.Framework;
using System;
using Xamarin.UITest;
using CoffeeFilter.UITests.Shared;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Android;
using System.Linq;
using Xamarin.UITest.Queries;

namespace CoffeeFilter.UITests
{

	public enum Platform
	{
		Android,
		iOS
	}

	[TestFixture (Platform.iOS)]
	[TestFixture (Platform.Android)]
	public class CoffeeFilterTests
	{

		IApp app;
		Platform platform;

		public string PathToAPK { get; } = "../../../CoffeeFilter.Android/bin/UITest/com.refractored.coffeefilter-Signed.apk";

		public string BundleID { get; } = "com.xamarin.CoffeeFilterContaining";

		public CoffeeFilterTests (Platform plat)
		{
			platform = plat;
		}

		[SetUp]
		public void SetUp ()
		{
			switch (platform) {
			case Platform.Android:
				app = ConfigureAndroid ();
				break;
			case Platform.iOS:
				app = ConfigureiOS ();
				break;
			}
		}

		AndroidApp ConfigureAndroid ()
		{
			if (TestEnvironment.Platform == TestPlatform.Local) {
				return ConfigureApp
					.Android
					.ApkFile (PathToAPK)
					.ApiKey (UITestsHelpers.XTCApiKey)
					.StartApp ();
			} else {

				return ConfigureApp
					.Android
					.StartApp ();
			}
		}

		iOSApp ConfigureiOS ()
		{
			if (TestEnvironment.Platform == TestPlatform.Local) {
				return ConfigureApp
					.iOS
					.InstalledApp (BundleID)
					.DeviceIdentifier ("e575659cb63d148c5909e28f464466d782ec98b1")
					.ApiKey (UITestsHelpers.XTCApiKey)
					.StartApp ();
			} else {
				return ConfigureApp
					.iOS
					.StartApp ();
			}
		}

		[Test]
		public void CoffeeFilter_VerifyMainScreen_ShouldDisplay ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));

			app.Query (c => c.Marked ("action_search"));
			app.Query (c => c.Marked ("action_navigation"));

			app.Query (c => c.Class (platform == Platform.iOS ? "UIMapView" : "MapView"));
			app.Query (c => c.Marked ("top"));
			app.Query (c => c.Marked ("bottom"));
		}

		[Test]
		public void CoffeeFilter_CycleThroughLocations_ShouldChangeCurrentItem ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			AppResult name;
			if (platform == Platform.Android) {
				name = app.Query (c => c.Marked ("bottom")).FirstOrDefault ();
			} else {
				name = app.Query (c => c.Marked ("bottom").Child ()).FirstOrDefault ();
			}
			Assert.NotNull (name, "Data was not loaded properly");
			app.SwipeLeft ();
			AppResult newName;
			if (platform == Platform.Android) {
				newName = app.Query (c => c.Marked ("bottom")).FirstOrDefault ();
			} else {
				newName = app.Query (c => c.Marked ("bottom").Child ()).FirstOrDefault ();
			}
			Assert.AreNotSame (newName.Text, name.Text, string.Format ("Swipe did not work (swiped to {0} from {1}", newName, name));
		}

		[Test]
		public void CoffeeFilter_RefreshLocations_ShouldRefresh ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			if (platform == Platform.iOS) {
				app.WaitForElement (c => c.Marked ("Refresh"), "Timed out waiting for Refresh button", TimeSpan.FromSeconds (10));
				app.Tap (c => c.Marked ("Refresh"));
				app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to laod", TimeSpan.FromSeconds (60));
				Assert.That (app.Query (c => c.Marked ("bottom")).Any (), "Refresh was unsuccessful");
			} else {
				var screen = app.Query (c => c.Index (0)).FirstOrDefault ();
				app.DragCoordinates (screen.Rect.CenterX, screen.Rect.CenterY, screen.Rect.CenterX, screen.Rect.Height);
				app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to laod", TimeSpan.FromSeconds (60));
				Assert.That (app.Query (c => c.Marked ("bottom")).Any (), "Refresh was unsuccessful");
			}
		}

		[Test]
		public void CoffeeFilter_SearchForLocation_ShouldDisplay ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			app.WaitForElement (c => c.Marked ("Search"), "Timed out waiting for Search button", TimeSpan.FromSeconds (10));
			app.Tap (c => c.Marked ("Search"));
			const string enteredText = "Espresso";
			app.EnterText (enteredText);
			app.PressEnter ();
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			AppResult name;
			if (platform == Platform.Android) {
				name = app.Query (c => c.Marked ("bottom")).FirstOrDefault ();
			} else {
				name = app.Query (c => c.Marked ("bottom").Child ()).FirstOrDefault ();
			}
			Assert.NotNull (name, "Data was not loaded properly");
			Assert.That (name.Text.Contains (enteredText), "Relevant entry was not loaded first");
		}

		[Test]
		public void CoffeeFilter_VerifyProductScreen_ShouldDisplay ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			app.Tap (c => c.Marked ("bottom"));
			if (platform == Platform.Android) {
				app.WaitForElement (c => c.Marked ("rating"));
				Assert.That (app.Query (c => c.Marked ("rating")).Any ());
			} else {
				app.WaitForElement (c => c.Marked ("name"));
			}

			Assert.That (app.Query (c => c.Marked ("name")).Any ());
			Assert.That (app.Query (c => c.Marked ("address")).Any ());
			Assert.That (app.Query (c => c.Marked ("distance")).Any ());
			Assert.That (app.Query (c => c.Marked ("price_hours")).Any ());
			Assert.That (app.Query (c => c.Marked ("phone_number")).Any ());
			if (platform == Platform.Android)
				Assert.That (app.Query (c => c.Class ("MapView")).Any ());

			app.ScrollDown ();

			Assert.That (app.Query (c => c.Text ("Monday").Sibling ().Marked ("monday")).Any ());
			Assert.That (app.Query (c => c.Text ("Tuesday").Sibling ().Marked ("tuesday")).Any ());
			Assert.That (app.Query (c => c.Text ("Wednesday").Sibling ().Marked ("wednesday")).Any ());

			app.ScrollDown ();

			Assert.That (app.Query (c => c.Text ("Thursday").Sibling ().Marked ("thursday")).Any ());
			Assert.That (app.Query (c => c.Text ("Friday").Sibling ().Marked ("friday")).Any ());
			Assert.That (app.Query (c => c.Text ("Saturday").Sibling ().Marked ("saturday")).Any ());
			Assert.That (app.Query (c => c.Text ("Sunday").Sibling ().Marked ("sunday")).Any ());

			app.ScrollDown ();

			if (platform == Platform.Android) {
				Assert.That (app.Query (c => c.Marked ("website_header").Text ("Website").Sibling ().Marked ("website")).Any ());
				Assert.That (app.Query (c => c.Marked ("google_plus_header").Text ("Google+ Listing").Sibling ().Marked ("google_plus")).Any ());
				app.SwipeRight ();
			} else {
				//Assert.That (app.Query (c => c.Class ("UIMapView")).Any ());
				app.Tap (c => c.Text ("Reviews"));
			}

			if (app.Query (c => c.Marked ("none")).Any ()) {
				switch (platform) {
				case Platform.iOS:
					app.Query (c => c.Text ("There are no reviews available."));
					break;
				case Platform.Android:
					Assert.That (Convert.ToInt32 (app.Query (c => c.Class ("GridView").Marked ("grid").Invoke ("getCount")).FirstOrDefault ()) == 0);
					break;
				}
			} else {
				Assert.That (app.Query (c => c.Marked ("item_rating")).Any ());
				Assert.That (app.Query (c => c.Marked ("item_review")).Any ());
				Assert.That (app.Query (c => c.Marked ("item_author_name")).Any ());
				Assert.That (app.Query (c => c.Marked ("item_date")).Any ());
			}

			if (platform == Platform.Android) {
				app.SwipeRight ();
				Assert.GreaterOrEqual (Convert.ToInt32(app.Query (c => c.Class ("GridView").Marked ("grid").Invoke ("getCount")).FirstOrDefault()), app.Query (c => c.Class ("SquareImageView")).Length);
			} else {
				app.Tap (c => c.Text ("Photos"));
				Assert.That (app.Query (c => c.Class ("UIImageView")).Any ());
			}
			app.Back ();
		}

		[Test]
		public void CoffeeFilter_ShareLocationDetails_ShouldDisplay ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds (60));
			app.Tap (c => c.Marked ("bottom"));
			if (platform == Platform.Android) {
				app.WaitForElement (c => c.Marked ("Share with"), "Timed out waiting for Share button", TimeSpan.FromSeconds (10));
				app.Tap (c => c.Marked ("Share with"));
				app.WaitForElement (c => c.Marked ("See all"), "Timed out waiting for activity view", TimeSpan.FromSeconds (10));
				Assert.That (app.Query (c => c.Marked ("See all")).Any ());
			} else {
				app.WaitForElement (c => c.Marked ("Share"), "Timed out waiting for Share button", TimeSpan.FromSeconds (10));
				app.Tap (c => c.ClassFull ("UINavigationButton").Marked ("Share"));

				app.WaitForElement (c => c.Marked ("ActivityListView"), "Timed out waiting for Activity List View button ", TimeSpan.FromSeconds (10));
				Assert.That (app.Query (c => c.Marked ("ActivityListView")).Any ());
			}
		}
	}
}

