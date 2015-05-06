using System;
using UIKit;
using Foundation;

namespace CoffeeFilter.iOS
{
	public partial class WarningMessageView : UIView
	{
		public static WarningMessageView GetView (string warningText, NSObject owner)
		{
			var warningView = NSBundle.MainBundle.LoadNib ("WarningMessageView", owner, null).GetItem<WarningMessageView> (0);
			warningView.WarningLabel.Text = warningText;
			return warningView;
		}

		public WarningMessageView (IntPtr handle) : base (handle)
		{
		}
	}
}

