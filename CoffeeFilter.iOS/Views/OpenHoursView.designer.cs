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
	[Register ("OpenHoursView")]
	partial class OpenHoursView
	{
		[Outlet]
		public UIKit.UILabel DayLabel { get; private set; }

		[Outlet]
		public UIKit.UILabel HoursLabel { get; private set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DayLabel != null) {
				DayLabel.Dispose ();
				DayLabel = null;
			}

			if (HoursLabel != null) {
				HoursLabel.Dispose ();
				HoursLabel = null;
			}
		}
	}
}
