// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CoffeeFilter.iOSWatchKitExtension
{
	[Register ("ReviewRowController")]
	partial class ReviewRowController
	{
		[Outlet]
		WatchKit.WKInterfaceLabel AuthorNameLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel DateLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel ReviewLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage star_0 { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage star_1 { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage star_2 { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage star_3 { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage star_4 { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ReviewLabel != null) {
				ReviewLabel.Dispose ();
				ReviewLabel = null;
			}

			if (AuthorNameLabel != null) {
				AuthorNameLabel.Dispose ();
				AuthorNameLabel = null;
			}

			if (DateLabel != null) {
				DateLabel.Dispose ();
				DateLabel = null;
			}

			if (star_0 != null) {
				star_0.Dispose ();
				star_0 = null;
			}

			if (star_1 != null) {
				star_1.Dispose ();
				star_1 = null;
			}

			if (star_2 != null) {
				star_2.Dispose ();
				star_2 = null;
			}

			if (star_3 != null) {
				star_3.Dispose ();
				star_3 = null;
			}

			if (star_4 != null) {
				star_4.Dispose ();
				star_4 = null;
			}
		}
	}
}
