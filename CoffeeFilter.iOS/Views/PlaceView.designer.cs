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
	[Register ("PlaceView")]
	partial class PlaceView
	{
		[Outlet]
		UIKit.UILabel DistanceLabel { get; set; }

		[Outlet]
		UIKit.UIButton placeButton { get; set; }

		[Outlet]
		UIKit.UILabel RatingLabel { get; set; }

		[Outlet]
		UIKit.UIImageView StarImageView { get; set; }

		[Action ("placeButtonTouchUpInside:")]
		partial void placeButtonTouchUpInside (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (DistanceLabel != null) {
				DistanceLabel.Dispose ();
				DistanceLabel = null;
			}

			if (placeButton != null) {
				placeButton.Dispose ();
				placeButton = null;
			}

			if (RatingLabel != null) {
				RatingLabel.Dispose ();
				RatingLabel = null;
			}

			if (StarImageView != null) {
				StarImageView.Dispose ();
				StarImageView = null;
			}
		}
	}
}
