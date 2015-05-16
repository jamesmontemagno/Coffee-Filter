// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CoffeeFilter.iOS
{
	[Register ("DetailsViewController")]
	partial class DetailsViewController
	{
		[Outlet]
		CoffeeFilter.iOS.PlaceDetailsCell InformationCell { get; set; }

		[Outlet]
		MapKit.MKMapView MapView { get; set; }

		[Outlet]
		UIKit.UITableViewCell OpenHoursCell { get; set; }

		[Outlet]
		UIKit.UIView ParallaxHeader { get; set; }

		[Outlet]
		UIKit.UIImageView ParallaxHeaderImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (InformationCell != null) {
				InformationCell.Dispose ();
				InformationCell = null;
			}

			if (MapView != null) {
				MapView.Dispose ();
				MapView = null;
			}

			if (OpenHoursCell != null) {
				OpenHoursCell.Dispose ();
				OpenHoursCell = null;
			}

			if (ParallaxHeader != null) {
				ParallaxHeader.Dispose ();
				ParallaxHeader = null;
			}

			if (ParallaxHeaderImageView != null) {
				ParallaxHeaderImageView.Dispose ();
				ParallaxHeaderImageView = null;
			}
		}
	}
}
