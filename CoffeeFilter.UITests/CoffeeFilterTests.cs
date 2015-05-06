using NUnit.Framework;
using System;
using Xamarin.UITest;
using CoffeeFilter.UITests.Shared;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Android;
using System.Linq;

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

		public string PathToAPK { get; } = "../../../CoffeeFilter.Android/bin/Release/com.refractored.coffeefilter-Signed.apk";

		public string BundleID { get; } = "com.xamarin.CoffeeFilter";

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
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds(60));

			app.Query (c => c.Marked ("action_search"));
			app.Query (c => c.Marked ("action_navigation"));

			app.Query (c => c.Class (platform == Platform.iOS ? "UIMapView" : "MapView"));
			app.Query (c => c.Marked ("top"));
			app.Query (c => c.Marked ("bottom"));
		}

		[Test]
		public void CoffeeFilter_CycleThroughLocations_ShouldChangeCurrentItem () {

		}

		[Test]
		public void CoffeeFilter_RefreshLocations_ShouldRefresh () {
			app.Repl ();
		}

		[Test]
		public void CoffeeFilter_SearchForLocation_ShouldDisplay () {
		}

		[Test]
		public void CoffeeFilter_VerifyProductScreen_ShouldDisplay ()
		{
			app.WaitForElement (c => c.Marked ("bottom"), "Timed out waiting for data to load", TimeSpan.FromSeconds(60));
			app.Tap (c => c.Marked ("bottom"));

			app.WaitForElement (c => c.Id ("rating"));
			Assert.That (app.Query (c => c.Marked ("rating")).Any ());

			Assert.That (app.Query (c => c.Marked ("name")).Any ());
			Assert.That (app.Query (c => c.Marked ("address")).Any ());
			Assert.That (app.Query (c => c.Marked ("distance")).Any ());
			Assert.That (app.Query (c => c.Marked ("price_hours")).Any ());
			Assert.That (app.Query (c => c.Marked ("phone_number")).Any ());
			Assert.That (app.Query (c => c.Class (platform == Platform.iOS ? "UIMapView" : "MapView")).Any ());

			app.ScrollDown ();

			Assert.That (app.Query (c => c.Text ("Monday").Sibling ().Marked ("monday")).Any ());
			Assert.That (app.Query (c => c.Text ("Tuesday").Sibling ().Marked ("tuesday")).Any ());
			Assert.That (app.Query (c => c.Text ("Wednesday").Sibling ().Marked ("wednesday")).Any ());
			Assert.That (app.Query (c => c.Text ("Thursday").Sibling ().Marked ("thursday")).Any ());
			Assert.That (app.Query (c => c.Text ("Friday").Sibling ().Marked ("friday")).Any ());
			Assert.That (app.Query (c => c.Text ("Saturday").Sibling ().Marked ("saturday")).Any ());
			Assert.That (app.Query (c => c.Text ("Sunday").Sibling ().Marked ("sunday")).Any ());

			app.ScrollDown ();

			Assert.That (app.Query (c => c.Marked ("website_header").Text ("Website").Sibling ().Marked ("website")).Any ());
			Assert.That (app.Query (c => c.Marked ("google_plus_header").Text ("Google+ Listing").Sibling ().Marked ("google_plus")).Any ());

			app.SwipeRight ();

			if (app.Query (c => c.Marked ("none")).Any ()) {
				switch (platform) {
				case Platform.iOS:
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

			app.SwipeRight ();

			Assert.AreEqual (app.Query (c => c.Class ("GridView").Marked ("grid").Invoke ("getCount")) [0], app.Query (c => c.Class ("SquareImageView")).Length);

			app.Back ();
		}

		[Test]
		public void CoffeeFilter_ShareLocationDetails_ShouldDisplay () {
		}
	}
}

