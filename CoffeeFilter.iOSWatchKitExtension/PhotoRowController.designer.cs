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
	[Register ("PhotoRowController")]
	partial class PhotoRowController
	{
		[Outlet]
		WatchKit.WKInterfaceImage LoadingAnimation { get; set; }

		[Outlet]
		WatchKit.WKInterfaceImage PhotoView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoadingAnimation != null) {
				LoadingAnimation.Dispose ();
				LoadingAnimation = null;
			}

			if (PhotoView != null) {
				PhotoView.Dispose ();
				PhotoView = null;
			}
		}
	}
}
