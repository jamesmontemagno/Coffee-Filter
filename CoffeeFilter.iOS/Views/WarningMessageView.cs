using System;
using UIKit;
using Foundation;

namespace CoffeeFilter.iOS
{
	public partial class WarningMessageView : UIView
	{
		public WarningMessageView (IntPtr handle) : base(handle)
		{
		}

		public static WarningMessageView GetView (string warningText, NSObject owner)
		{
			var warningView = NSBundle.MainBundle.LoadNib<WarningMessageView>(owner);
		
			warningView.WarningLabel.Text = warningText;

			return warningView;
		}
	}
}