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
	[Register ("ReviewCell")]
	partial class ReviewCell
	{
		[Outlet]
		UIKit.UILabel AuthorNameLabel { get; set; }

		[Outlet]
		CoffeeFilter.iOS.RatingView RatingControl { get; set; }

		[Outlet]
		UIKit.UILabel ReviewDateLabel { get; set; }

		[Outlet]
		UIKit.UILabel ReviewLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AuthorNameLabel != null) {
				AuthorNameLabel.Dispose ();
				AuthorNameLabel = null;
			}

			if (RatingControl != null) {
				RatingControl.Dispose ();
				RatingControl = null;
			}

			if (ReviewDateLabel != null) {
				ReviewDateLabel.Dispose ();
				ReviewDateLabel = null;
			}

			if (ReviewLabel != null) {
				ReviewLabel.Dispose ();
				ReviewLabel = null;
			}
		}
	}
}
