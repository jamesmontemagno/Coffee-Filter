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
	[Register ("PlaceDetailsCell")]
	partial class PlaceDetailsCell
	{
		[Outlet]
		MapKit.MKMapView MapView { get; set; }

		[Outlet]
		UIKit.UILabel PlaceAddress { get; set; }

		[Outlet]
		UIKit.UILabel PlaceDistance { get; set; }

		[Outlet]
		UIKit.UILabel PlaceName { get; set; }

		[Outlet]
		UIKit.UILabel PlaceOpenHours { get; set; }

		[Outlet]
		UIKit.UILabel PlacePhone { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (PlaceName != null) {
				PlaceName.Dispose ();
				PlaceName = null;
			}

			if (PlaceAddress != null) {
				PlaceAddress.Dispose ();
				PlaceAddress = null;
			}

			if (PlaceDistance != null) {
				PlaceDistance.Dispose ();
				PlaceDistance = null;
			}

			if (PlaceOpenHours != null) {
				PlaceOpenHours.Dispose ();
				PlaceOpenHours = null;
			}

			if (PlacePhone != null) {
				PlacePhone.Dispose ();
				PlacePhone = null;
			}

			if (MapView != null) {
				MapView.Dispose ();
				MapView = null;
			}
		}
	}
}
