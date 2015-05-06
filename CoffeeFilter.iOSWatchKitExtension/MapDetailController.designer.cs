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
	[Register ("MapDetailController")]
	partial class MapDetailController
	{
		[Outlet]
		WatchKit.WKInterfaceLabel DistanceLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceMap MapView { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton PhotosButton { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel PlaceNameLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel RatingLabel { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton ReviewsButton { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage StartImage { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DistanceLabel != null) {
				DistanceLabel.Dispose ();
				DistanceLabel = null;
			}

			if (MapView != null) {
				MapView.Dispose ();
				MapView = null;
			}

			if (PhotosButton != null) {
				PhotosButton.Dispose ();
				PhotosButton = null;
			}

			if (RatingLabel != null) {
				RatingLabel.Dispose ();
				RatingLabel = null;
			}

			if (ReviewsButton != null) {
				ReviewsButton.Dispose ();
				ReviewsButton = null;
			}

			if (StartImage != null) {
				StartImage.Dispose ();
				StartImage = null;
			}

			if (PlaceNameLabel != null) {
				PlaceNameLabel.Dispose ();
				PlaceNameLabel = null;
			}
		}
	}
}
