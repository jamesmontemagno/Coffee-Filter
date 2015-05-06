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
	[Register ("InterfaceController")]
	partial class InterfaceController
	{
		[Outlet]
		WatchKit.WKInterfaceImage CoffeeAnimationView { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup ImageGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceTable PlacesTable { get; set; }

		[Outlet]
		WatchKit.WKInterfaceButton TryAgainButton { get; set; }

		[Outlet]
		WatchKit.WKInterfaceGroup WarningGroup { get; set; }

		[Outlet]
		WatchKit.WKInterfaceLabel WarningMessageLabel { get; set; }

		[Action ("TryAgainButtonClick")]
		partial void TryAgainButtonClick ();
		
		void ReleaseDesignerOutlets ()
		{
			if (CoffeeAnimationView != null) {
				CoffeeAnimationView.Dispose ();
				CoffeeAnimationView = null;
			}

			if (PlacesTable != null) {
				PlacesTable.Dispose ();
				PlacesTable = null;
			}

			if (ImageGroup != null) {
				ImageGroup.Dispose ();
				ImageGroup = null;
			}

			if (WarningGroup != null) {
				WarningGroup.Dispose ();
				WarningGroup = null;
			}

			if (WarningMessageLabel != null) {
				WarningMessageLabel.Dispose ();
				WarningMessageLabel = null;
			}

			if (TryAgainButton != null) {
				TryAgainButton.Dispose ();
				TryAgainButton = null;
			}
		}
	}
}
