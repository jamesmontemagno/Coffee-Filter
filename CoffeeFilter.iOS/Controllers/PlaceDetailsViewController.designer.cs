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
	[Register ("PlaceDetailsViewController")]
	partial class PlaceDetailsViewController
	{
		[Outlet]
		UIKit.UITabBar TabBar { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TabBar != null) {
				TabBar.Dispose ();
				TabBar = null;
			}
		}
	}
}
