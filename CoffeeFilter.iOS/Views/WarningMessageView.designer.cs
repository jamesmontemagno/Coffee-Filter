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
	[Register ("WarningMessageView")]
	partial class WarningMessageView
	{
		[Outlet]
		UIKit.UILabel WarningLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (WarningLabel != null) {
				WarningLabel.Dispose ();
				WarningLabel = null;
			}
		}
	}
}
